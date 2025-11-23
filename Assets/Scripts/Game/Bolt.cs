using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bolt : MonoBehaviour
{
    [SerializeField] private ScrewPosition[] _screwPositions;
    [SerializeField] private Transform _topPosition;
    [SerializeField] private float _screwVerticalTime;
    [SerializeField] private float _screwRotationTime;
    [SerializeField] private float _screwToBoltTime;
    [SerializeField] private float _screwAnimationScale;
    [SerializeField] private float _screwAnimationTime;
    [SerializeField] private float _screwAnimationDelta;
    [SerializeField] private float _screwFloatingDistance;
    [SerializeField] private float _screwAnimFloatingTime;
    [SerializeField] private float _screwAnimRotationTime;

    private List<Screw> _movingScrews = new();
    private Level _level;
    private bool _isComplete;
    private bool _isLockedTake;
    private bool _isLockedPlace;
    private WaitForSeconds _screwAnimationWait;

    public float ScrewVerticalTime => _screwVerticalTime;
    public int ScrewPositionsCount => _screwPositions.Length;
    public bool IsMovingScrew => _movingScrews.Count > 0;
    public Vector3 TopPosition => _topPosition.position;
    public Level Level => _level;
    public bool IsComplete => _isComplete;
    public bool IsLockedPick => _isLockedTake;
    public bool IsLockedPlace => _isLockedPlace;

    private void Awake()
    {
        _screwAnimationWait = new(_screwAnimationDelta);
    }

    public Screw GetUpperScrew()
    {
        for(int i = _screwPositions.Length - 1; i >=0; i--)
        {
            if (_screwPositions[i].Screw != null)
                return _screwPositions[i].Screw;
        }

        return null;
    }

    public void LiftScrew(Screw screwToLift)
    {
        for (int i = _screwPositions.Length - 1; i >= 0; i--)
        {
            if (_screwPositions[i].Screw == screwToLift)
            {
                _screwPositions[i].Screw.transform.DOKill();
                _screwPositions[i].Screw.transform.transform.DOLocalRotate(new Vector3(0, -360, 0), _screwRotationTime, RotateMode.FastBeyond360);
                int screwIndex = i;
                _screwPositions[i].Screw.transform.DOMove(TopPosition, _screwVerticalTime).OnComplete(()=> OnScrewLifted(screwToLift, screwIndex));
            }
        }
    }

    public void MoveScrewToBolt(Screw screw)
    {
        _movingScrews.Add(screw);

        if (DOTween.IsTweening(screw.transform))
        {
            Tween currentTween = DOTween.TweensByTarget(screw.transform)?[0];

            if (currentTween != null)
                currentTween.OnComplete(() => screw.transform.DOMove(TopPosition, _screwToBoltTime).OnComplete(() => PlaceScrew(screw)));
        }
        else
        {
            screw.transform.DOMove(TopPosition, _screwToBoltTime).OnComplete(() => PlaceScrew(screw));
        }
    }

    public void PlaceScrew(Screw screw)
    {
        foreach (ScrewPosition screwPosition in _screwPositions)
        {
            if (screwPosition.Screw != null && screwPosition.Screw != screw)
                continue;

            _movingScrews.Remove(screw);
            screw.transform.DOKill();
            screw.transform.SetParent(screwPosition.Transform);
            screw.transform.DOMove(screwPosition.Transform.position, _screwVerticalTime).OnComplete(()=> OnScrewAdded(screw));
            screw.transform.transform.DOLocalRotate(new Vector3(0, 360, 0), _screwRotationTime, RotateMode.FastBeyond360);
            screwPosition.Screw = screw;
            CheckCompletion();
            break;
        }
    }

    public void RemoveScrew(Screw screw)
    {
        foreach (ScrewPosition position in _screwPositions)
        {
            if (position.Screw == screw)
            {
                position.Screw = null;
                return;
            }
        }
    }

    public bool CanPlaceScrew(Screw screw)
    {
        int occupiedPlaces = 0;
        bool isSameIndex = false;

        foreach (ScrewPosition position in _screwPositions)
        {
            if (position.Screw != null)
            {
                if (position.Screw.Index == screw.Index)
                {
                    isSameIndex = true;
                }
                else
                {
                    isSameIndex = false;
                }

                occupiedPlaces++;
            }
        }

        foreach(Screw movingScrew in _movingScrews)
        {
            if (movingScrew.Index != screw.Index)
                return false;
        }

        if(occupiedPlaces + _movingScrews.Count == _screwPositions.Length  || (!isSameIndex && occupiedPlaces>0))
            return false;

        return true;
    }

    public int EmptyPlacesAmount()
    {
        int emptyPlaces = 0;

        foreach (ScrewPosition position in _screwPositions)
        {
            if (position.Screw == null)
                emptyPlaces++;
        }

        return emptyPlaces;
    }

    public bool IsEmpty()
    {
        return EmptyPlacesAmount() == _screwPositions.Length;
    }

    public void SetLevel(Level level)
    {
        _level = level;
    }

    public List<Screw> GetSameScrews(int maxSameScrews)
    {
        List<Screw> screws = new();
        int screwsIndex = -1;
        bool allSame = true;
        
        for (int i = 0;i< _screwPositions.Length; i++)
        {
            if (_screwPositions[i].Screw != null)
            {
                screws.Add(_screwPositions[i].Screw);

                if (i == 0)
                {
                    screwsIndex = _screwPositions[i].Screw.Index;
                    continue;
                }

                if(_screwPositions[i].Screw.Index!=screwsIndex)
                    allSame = false;
            }
            else
            {
                break;
            }
        }

        if(allSame && screws.Count >=  maxSameScrews)
            screws.Clear();

        return screws;
    }

    public void CheckCompletion()
    {
        if (!_level.IsActive)
            return;

        if (IsBoltComplete())
        {
            _isComplete = true;
            StartCoroutine(ScrewsAnimation());
        }

        BoostsManager.Instance.CheckReturnButton(false);
    }

    public void LockTakingScrews() //For tutorial only
    {
        _isLockedTake = true;
    }

    public void LockPlacingScrews() //For tutorial only
    {
        _isLockedPlace = true;
    }

    private void OnScrewAdded(Screw screw)
    {
        CheckCompletion();
    }

    private void OnScrewLifted(Screw screw, int index)
    {
        if (_screwPositions[index].Screw == null || screw == null)
            return;

        if (_screwPositions[index].Screw == screw)
        {
            _screwPositions[index].Screw.transform.DOLocalRotate(new Vector3(0, 30, 0), _screwAnimRotationTime).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
            _screwPositions[index].Screw.transform.DOMoveY(_screwPositions[index].Screw.transform.position.y + _screwFloatingDistance, _screwAnimFloatingTime).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        }
    }

    private IEnumerator ScrewsAnimation()
    {
        foreach (ScrewPosition screwPosition in _screwPositions)
        {
            screwPosition.Screw.transform.DOScale(_screwAnimationScale, _screwAnimationTime).SetLoops(2, LoopType.Yoyo);
            yield return _screwAnimationWait;
        }

        EventBus.OnBoltCompleted?.Invoke(this);
        _level.BoltCompleted();
    }

    private bool IsBoltComplete()
    {
        if (_screwPositions[0].Screw == null || IsMovingScrew || _isComplete)
            return false;

        int firstScrewIndex = _screwPositions[0].Screw.Index;

        for (int i = 1; i < _screwPositions.Length; i++)
        {
            if (_screwPositions[i].Screw == null)
                return false;

            if (_screwPositions[i].Screw.Index != firstScrewIndex)
                return false;
        }

        return true;
    }

    private void OnAddBoltBoost()
    {
        foreach (ScrewPosition screwPosition in _screwPositions)
        {
            if (screwPosition.Screw == null)
                break;

            if(screwPosition.Screw.transform.position != screwPosition.Transform.position)
                screwPosition.Screw.transform.DOMove(screwPosition.Transform.position, _screwVerticalTime);
        }
    }

    private void OnEnable()
    {
        EventBus.OnAddBoltBoost += OnAddBoltBoost;
    }

    private void OnDisable()
    {
        EventBus.OnAddBoltBoost -= OnAddBoltBoost;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _screwPositions.OrderByDescending(screwPosition => screwPosition.Transform.position.y);
    }
#endif
}

[Serializable]
public class ScrewPosition
{
    public Screw Screw;
    public Transform Transform;
}