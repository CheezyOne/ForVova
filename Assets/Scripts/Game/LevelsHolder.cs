using DG.Tweening;
using System.Collections;
using UnityEngine;

public class LevelsHolder : MonoBehaviour
{
    [SerializeField] private Level _level;
    [SerializeField] private Level _skyscraperLevel;
    [SerializeField] private Level _shortLevel;
    [SerializeField] private Camera _mainCamera;

    [Header("Infinite levels")]
    [SerializeField] private Transform _newLevelPosition;
    [SerializeField] private Transform _completeLevelDestination;
    [SerializeField] private float _levelMoveTime;

    private Level _currentLevel;
    private Level _nextLevel;
    private WaitForEndOfFrame _waitFrame = new();

    public Level CurrentLevel => _currentLevel;

    private void Awake()
    {
        LevelsManager.Instance.ResetInfiniteCounter();

        if (LevelsManager.Instance.GameMode == GameMode.Infinite)
        {
            StartCoroutine(SpawnInfiniteLevel());
            StartCoroutine(SpawnInfiniteLevel());
        }
        else
        {
            Level newLevel = null;

            if (LevelsManager.Instance.IsDifficultLevel)
            {
                if(SaveLoadSystem.data.LevelType == DifficultLevelType.Skyscraper)
                {
                    newLevel = Instantiate(_skyscraperLevel, transform.position, Quaternion.identity, transform);
                }
                else if(SaveLoadSystem.data.LevelType == DifficultLevelType.Short)
                {
                    newLevel = Instantiate(_shortLevel, transform.position, Quaternion.identity, transform);
                }
            }

            if(newLevel == null)
                newLevel = Instantiate(_level, transform.position, Quaternion.identity, transform);

            _currentLevel = newLevel;
            newLevel.SetLevelData(SaveLoadSystem.data.LevelData.Count, _mainCamera);
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
            OnLevelCompleted();
    }
#endif

    private IEnumerator SpawnInfiniteLevel()
    {
        Level newLevel;
        bool activeOnCompletion = false;

        if(_currentLevel == null)
        {
            newLevel = Instantiate(_level, transform.position, Quaternion.identity,  transform);
            _currentLevel = newLevel;
            activeOnCompletion = true;
        }
        else if(_nextLevel == null)
        {
            newLevel = Instantiate(_level, _newLevelPosition.position, Quaternion.identity, transform);
            _nextLevel = newLevel;
        }
        else
        {
            while(!_nextLevel.AreScrewsReady)
            {
                yield return _waitFrame;
            }

            newLevel = Instantiate(_level, _newLevelPosition.position, Quaternion.identity, transform);
            GameObject completeLevel = _currentLevel.gameObject;
            _currentLevel.transform.DOMove(_completeLevelDestination.position, _levelMoveTime).OnComplete(() => Destroy(completeLevel));
            Level nextLevel = _nextLevel;
            _nextLevel.transform.DOMove(transform.position, _levelMoveTime).OnComplete(() => nextLevel.ActivateLevel());
            _currentLevel = _nextLevel;
            _nextLevel = newLevel;
        }

        LevelData levelData = new();
        newLevel.SetLevelData(levelData.SavedBoltInfos.Count, _mainCamera, levelData, activeOnCompletion);
    }

    private void OnLevelCompleted()
    {
        if (LevelsManager.Instance.GameMode == GameMode.Infinite)
            StartCoroutine(SpawnInfiniteLevel());
    }

    private void OnEnable()
    {
        EventBus.OnLevelCompleted += OnLevelCompleted;
    }

    private void OnDisable()
    {
        EventBus.OnLevelCompleted -= OnLevelCompleted;
    }
}