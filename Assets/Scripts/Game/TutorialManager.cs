using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private TutorialHand _hand;
    [SerializeField] private Transform _handHolder;
    [SerializeField] private GameObject _cross;
    [SerializeField] private GameObject _check;
    [SerializeField] private LevelsHolder _levelsHolder;
    [SerializeField] private Vector3 _handOffset;

    private Bolt _selectedBolt;
    private List<GameObject> _currentCrosses = new();
    private List<GameObject> _currentChecks = new();
    private TutorialHand _currentHand;
    private WaitForEndOfFrame _waitFrame = new();

    private void Start() // Awaits level generation
    {
        if (SaveLoadSystem.data.CurrentLevel > 1)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void SetFirstTutorial()
    {
        if (_currentHand == null)
        {
            _levelsHolder.CurrentLevel.SpawnedBolts[0].LockPlacingScrews();
            _levelsHolder.CurrentLevel.SpawnedBolts[1].LockTakingScrews();
            _currentHand = Instantiate(_hand, _levelsHolder.CurrentLevel.SpawnedBolts[0].transform.position + _handOffset, Quaternion.identity, _handHolder);
        }
        else
        {
            Destroy(_currentHand.gameObject);
            _currentHand = Instantiate(_hand, _levelsHolder.CurrentLevel.SpawnedBolts[1].transform.position + _handOffset, Quaternion.identity, _handHolder);
        }
    }

    private void SetSecondTutorial()
    {
        ClearMarks();
        Screw selectedScrew = _selectedBolt.GetUpperScrew();

        foreach (Bolt bolt in _levelsHolder.CurrentLevel.SpawnedBolts)
        {
            if (_selectedBolt == bolt)
                continue;

            Screw boltUpperScrew = bolt.GetUpperScrew();

            if((boltUpperScrew == null || boltUpperScrew.Index == selectedScrew.Index) && bolt.EmptyPlacesAmount() != 0 && !bolt.IsMovingScrew)
            {
                _currentChecks.Add(Instantiate(_check, bolt.TopPosition, Quaternion.identity));
            }
            else
            {
                _currentCrosses.Add(Instantiate(_cross, bolt.TopPosition, Quaternion.identity));
            }
        }
    }

    private void ClearMarks()
    {
        foreach (GameObject cross in _currentCrosses)
        {
            Destroy(cross);
        }

        foreach (GameObject check in _currentChecks)
        {
            Destroy(check);
        }
    }

    private void OnScrewSelected(Bolt bolt)
    {
        if (SaveLoadSystem.data.CurrentLevel == 1)
        {
            _selectedBolt = bolt;
            SetSecondTutorial();
        }
        else if(SaveLoadSystem.data.CurrentLevel == 0)
        {
            SetFirstTutorial();
        }
    }

    private void EndFirstTutorial()
    {
        Destroy(_currentHand.gameObject);
    }

    private void OnShuffleBoost()
    {
        StartCoroutine(ShuffleBoostWait());
    }

    private IEnumerator ShuffleBoostWait()
    {
        yield return _waitFrame;
        SetSecondTutorial();
    }

    private void OnEnable()
    {
        if (SaveLoadSystem.data.CurrentLevel < 2)
            EventBus.OnScrewSelected += OnScrewSelected;

        if (SaveLoadSystem.data.CurrentLevel == 0)
        {
            EventBus.OnLevelGenerated += SetFirstTutorial;
            EventBus.OnScrewMoved += EndFirstTutorial;
        }
        else if(SaveLoadSystem.data.CurrentLevel == 1)
        {
            EventBus.OnScrewMoved += ClearMarks;
            EventBus.OnShuffleBoost += OnShuffleBoost;
        }
    }

    private void OnDisable()
    {
        EventBus.OnScrewSelected -= OnScrewSelected;
        EventBus.OnLevelGenerated -= SetFirstTutorial;
        EventBus.OnScrewMoved -= EndFirstTutorial;
        EventBus.OnScrewMoved -= ClearMarks;
        EventBus.OnShuffleBoost -= OnShuffleBoost;
    }
}