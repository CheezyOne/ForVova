using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private float _nextScrewDelta;

    private List<Screw> _movingScrews = new();
    private Screw _heldScrew;
    private Bolt _latestBolt;
    private WaitForSeconds _nextScrewWait;
    private LastMovedScrews _lastMovedScrews;
    private WaitForEndOfFrame _waitFrame = new();

    private void Awake()
    {
        _nextScrewWait = new(_nextScrewDelta);
    }

    public bool CanReturnScrews()
    {
        if (_lastMovedScrews == null || _movingScrews.Count > 0)
            return false;

        if (_lastMovedScrews.CurrentBolt.IsComplete || _lastMovedScrews.CurrentBolt.IsMovingScrew)
            return false;

        return true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnScreenInput(Input.mousePosition);
    }

    private void OnScreenInput(Vector2 screenPosition)
    {
        Ray ray = _mainCamera.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.TryGetComponent(out Bolt bolt))
            {
                if(bolt.IsComplete || !bolt.Level.IsActive)
                    return;

                if (bolt.IsLockedPick && _heldScrew == null || _heldScrew != null && bolt.IsLockedPlace)
                    return;

                Screw screw = bolt.GetUpperScrew();

                if (screw != null && bolt.IsMovingScrew && (_heldScrew == null || _heldScrew.Index != screw.Index))
                    return;

                if (_movingScrews.Contains(screw))
                {
                    ReturnHeldScrew();
                    return;
                }

                if (screw == null && bolt.IsEmpty() && _heldScrew == null)
                    return;

                if (_heldScrew == null)
                {
                    _latestBolt = bolt;
                    _heldScrew = bolt.GetUpperScrew();
                    bolt.LiftScrew(_heldScrew);
                    EventBus.OnScrewSelected?.Invoke(bolt);
                    return;
                }

                if (screw == _heldScrew)
                {
                    bolt.PlaceScrew(screw);
                    _heldScrew = null;
                    _latestBolt = null;
                    EventBus.OnScrewMoved?.Invoke();
                    return;
                }

                _heldScrew.transform.DOKill();

                if (bolt.CanPlaceScrew(_heldScrew))
                {
                    _lastMovedScrews = new();
                    _lastMovedScrews.LastBolt = _latestBolt;
                    _lastMovedScrews.CurrentBolt = bolt;
                    _lastMovedScrews.ScrewsAmount = 1;
                    _latestBolt.RemoveScrew(_heldScrew);
                    bolt.MoveScrewToBolt(_heldScrew);
                    StartCoroutine(MoveSameScrewsRoutine(_latestBolt, bolt, _heldScrew));
                }
                else
                {
                    _latestBolt.PlaceScrew(_heldScrew);
                }

                _heldScrew = null;
                _latestBolt = null;
                EventBus.OnScrewMoved?.Invoke();
            }
        }
    }

    private IEnumerator MoveSameScrewsRoutine(Bolt oldBolt, Bolt newBolt, Screw lastScrew)
    {
        int emptySpaces = newBolt.EmptyPlacesAmount();
        Screw nextScrew = oldBolt.GetUpperScrew();

        if (nextScrew == null)
            yield break;

        if (nextScrew.IsSecret)
        {
            nextScrew.RevealMaterial();
            yield break;
        }

        for (int i = 1; i < emptySpaces; i++)
        {
            if(nextScrew != null && !nextScrew.IsSecret)
            {
                if(nextScrew.Index == lastScrew.Index)
                {
                    _movingScrews.Add(nextScrew);
                }
                else
                {
                    nextScrew.RevealMaterial();
                    yield break;
                }
            }
            else
            {
                if (nextScrew != null)
                    nextScrew.RevealMaterial();

                yield break;
            }

            yield return _nextScrewWait;

            if (!newBolt.CanPlaceScrew(nextScrew))
            {
                _movingScrews.Remove(nextScrew);
                break;
            }

            _lastMovedScrews.ScrewsAmount++;
            oldBolt.LiftScrew(nextScrew);
            oldBolt.RemoveScrew(nextScrew);
            newBolt.MoveScrewToBolt(nextScrew);
            _movingScrews.Remove(nextScrew);
            nextScrew = oldBolt.GetUpperScrew();
        }

        if (nextScrew != null)
            nextScrew.RevealMaterial();
    }

    private void ReturnHeldScrew()
    {
        if (_heldScrew != null)
        {
            _latestBolt.PlaceScrew(_heldScrew);
            _heldScrew = null;
            _latestBolt = null;
        }
    }

    private void ReturnLastScrews()
    {
        StartCoroutine(ReturnLastScrewsRoutine());
    }

    private IEnumerator ReturnLastScrewsRoutine()
    {
        ReturnHeldScrew();

        for (int i = 0; i < _lastMovedScrews.ScrewsAmount; i++)
        {
            Screw nextScrew = _lastMovedScrews.CurrentBolt.GetUpperScrew();
            _movingScrews.Add(nextScrew);

            yield return _nextScrewWait;

            ReturnHeldScrew();
            _lastMovedScrews.CurrentBolt.LiftScrew(nextScrew);
            _lastMovedScrews.CurrentBolt.RemoveScrew(nextScrew);
            _lastMovedScrews.LastBolt.MoveScrewToBolt(nextScrew);
            _movingScrews.Remove(nextScrew);
        }

        _lastMovedScrews = null;
    }

    private void OnShuffleBoost()
    {
        _lastMovedScrews = null;
        BoostsManager.Instance.CheckReturnButton();
    }

    private void OnBoltCompleted(Bolt bolt)
    {
        if(bolt.GetUpperScrew() == _heldScrew)
        {
            bolt.PlaceScrew(_heldScrew);
            _heldScrew = null;
        }
    }

    private void OnAddBoltBoost()
    {
        StartCoroutine(ReturnHeldScrewRoutine());
    }

    private IEnumerator ReturnHeldScrewRoutine()
    {
        yield return _waitFrame;
        ReturnHeldScrew();
    }

    private void OnEnable()
    {
        EventBus.OnScrewReturnBoost += ReturnLastScrews;
        EventBus.OnBoltCompleted += OnBoltCompleted;
        EventBus.OnShuffleBoost += OnShuffleBoost;
        EventBus.OnAddBoltBoost += OnAddBoltBoost;
    }

    private void OnDisable()
    {
        EventBus.OnScrewReturnBoost -= ReturnLastScrews;
        EventBus.OnBoltCompleted -= OnBoltCompleted;
        EventBus.OnShuffleBoost -= OnShuffleBoost;
        EventBus.OnAddBoltBoost -= OnAddBoltBoost;
    }
}

public class LastMovedScrews
{
    public Bolt LastBolt;
    public Bolt CurrentBolt;
    public int ScrewsAmount;
}