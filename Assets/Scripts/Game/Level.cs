using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    [SerializeField] private Transform _boltsPlace;
    [SerializeField] private Bolt _normalBolt;
    [SerializeField] private Bolt _shortBolt;
    [SerializeField] private Bolt _tallBolt;
    [SerializeField] private Bolt _skyscraperBolt;
    [SerializeField] private float _spawnTimeDelta;
    [SerializeField] private float _transparency;
    [SerializeField] private Screw _screw;
    [SerializeField] private ScrewsConfig _screwsConfig;
    [SerializeField] private int _shuffleMaxSameScrews;
    [SerializeField] private Vector3 _gridCellSize;
    [SerializeField] private int _gridColumns = 4;
    [SerializeField] private float _distanceOffsetPerRow;
    [SerializeField] private float _zOffset;
    [SerializeField] private float _bottomRowCameraOffset = 0.5f;

    private Camera _mainCamera; 
    private WaitForSeconds _spawnWait;
    private List<Bolt> _spawnedBolts = new();
    private int _currentScrewIndex;
    private int _necesaryBolts;
    private int _completeBoltsCounter;
    private bool _isActive;
    private bool _areScrewsReady;
    private Bolt _levelBolt;
    private List<Vector3> _boltPositions = new List<Vector3>();

    public List<Bolt> SpawnedBolts => _spawnedBolts;
    public bool IsActive => _isActive;
    public bool AreScrewsReady => _areScrewsReady;

    private void Awake()
    {
        _spawnWait = new(_spawnTimeDelta);
    }

    public void SetLevelData(int boltsCount, Camera mainCamera, LevelData levelData = null, bool activeOnCompletion = false)
    {
        _levelBolt = _normalBolt;
        _mainCamera = mainCamera;

        if (!LevelsManager.Instance.IsDifficultLevel || LevelsManager.Instance.GameMode == GameMode.Infinite)
        {
            _levelBolt = _normalBolt;
        }
        else
        {
            switch (SaveLoadSystem.data.LevelType)
            {
                case DifficultLevelType.Short:
                    {
                        _levelBolt = _shortBolt;
                        break;
                    }
                case DifficultLevelType.Tall:
                    {
                        _levelBolt = _tallBolt;
                        break;
                    }
                case DifficultLevelType.Skyscraper:
                    {
                        _levelBolt = _skyscraperBolt;
                        break;
                    }
                default:
                    {
                        _levelBolt = _normalBolt;
                        break;
                    }
            }
        }

        CalculateBoltPositions(boltsCount);

        for (int i = 0; i < boltsCount; i++)
        {
            Bolt newBolt = Instantiate(_levelBolt, _boltsPlace);
            newBolt.transform.localPosition = _boltPositions[i];
            _spawnedBolts.Add(newBolt);
            newBolt.SetLevel(this);
        }

        if (levelData != null)
        {
            GetNecessaryBoltsCount(levelData.SavedBoltInfos);
            StartCoroutine(InfiniteScrewsSpawnRoutine(levelData, activeOnCompletion));
        }
        else
        {
            GetNecessaryBoltsCount(SaveLoadSystem.data.LevelData);
            StartCoroutine(LevelsScrewsSpawnRoutine());
        }
    }

    public void BoltCompleted()
    {
        _completeBoltsCounter++;

        if(_completeBoltsCounter == _necesaryBolts)
            EventBus.OnLevelCompleted?.Invoke();
    }

    public void ActivateLevel()
    {
        _isActive = true;
        BoostsManager.Instance.EnableButtons();
        EventBus.OnLevelGenerated?.Invoke();

        foreach(Bolt bolt in _spawnedBolts)
        {
            bolt.CheckCompletion();
        }
    }

    private void CalculateBoltPositions(int boltCount)
    {
        _boltPositions.Clear();

        if (boltCount == 0)
            return;

        int columns = _gridColumns;
        int fullRows = boltCount / columns;
        int remainingBolts = boltCount % columns;
        int totalRows = fullRows + (remainingBolts > 0 ? 1 : 0);
        float totalHeight = (totalRows - 1) * _gridCellSize.y;
        float startY = totalHeight * 0.5f;
        float startX;

        if (totalRows == 1)
        {
            float rowWidth = (boltCount - 1) * _gridCellSize.x;
            startX = -rowWidth * 0.5f;
        }
        else
        {
            float firstRowWidth = (columns - 1) * _gridCellSize.x;
            startX = -firstRowWidth * 0.5f;
        }

        Vector3 cameraToCenter = transform.position - _mainCamera.transform.position;
        Vector3 cameraDirection = new Vector3(0f, cameraToCenter.y, cameraToCenter.z);
        cameraDirection.Normalize();

        for (int i = 0; i < boltCount; i++)
        {
            int row = i / columns;
            int col = i % columns;
            Vector3 basePosition = new Vector3(startX + col * _gridCellSize.x, startY - row * _gridCellSize.y, -_zOffset);
            float rowOffset = _bottomRowCameraOffset + (totalRows - 1 - row) * _distanceOffsetPerRow;
            Vector3 distanceOffset = cameraDirection * rowOffset;
            Vector3 position = basePosition + distanceOffset;
            _boltPositions.Add(position);
        }
    }

    private IEnumerator InfiniteScrewsSpawnRoutine(LevelData levelData, bool activeOnCompletion = false)
    {
        int maxScrewsCount = levelData.SavedBoltInfos[0].ScrewsIndexes.Length;

        while (_currentScrewIndex < maxScrewsCount)
        {
            yield return _spawnWait;

            for (int i = 0; i < _spawnedBolts.Count; i++)
            {
                if (_currentScrewIndex < levelData.SavedBoltInfos[i].ScrewsIndexes.Length)
                {
                    Screw newScrew = Instantiate(_screw, _spawnedBolts[i].TopPosition, _screw.transform.rotation, transform);
                    newScrew.SetInfo(levelData.SavedBoltInfos[i].ScrewsIndexes[_currentScrewIndex], _screwsConfig.Materials[levelData.SavedBoltInfos[i].ScrewsIndexes[_currentScrewIndex]]);
                    _spawnedBolts[i].PlaceScrew(newScrew);
                }
            }

            _currentScrewIndex++;
        }

        if (activeOnCompletion)
        {
            ActivateLevel();
        }
        else
        {
            yield return new WaitForSeconds(_levelBolt.ScrewVerticalTime);
            _areScrewsReady = true;
        }
    }

    private IEnumerator LevelsScrewsSpawnRoutine()
    {
        int maxScrewsCount = GetLevelsMaxScrewAmount();

        while (_currentScrewIndex < maxScrewsCount)
        {
            yield return _spawnWait;

            for (int i = 0; i < _spawnedBolts.Count; i++)
            {
                if (_currentScrewIndex < SaveLoadSystem.data.LevelData[i].ScrewsIndexes.Length)
                {
                    Screw newScrew = Instantiate(_screw, _spawnedBolts[i].TopPosition, _screw.transform.rotation);

                    if (LevelsManager.Instance.IsDifficultLevel && SaveLoadSystem.data.LevelType == DifficultLevelType.Secret && _currentScrewIndex < maxScrewsCount - 1 && (_currentScrewIndex) % 2 == 0)
                    {
                        newScrew.SetInfo(SaveLoadSystem.data.LevelData[i].ScrewsIndexes[_currentScrewIndex], _screwsConfig.Materials[SaveLoadSystem.data.LevelData[i].ScrewsIndexes[_currentScrewIndex]], _screwsConfig.SecretMaterial);
                    }
                    else
                    {
                        newScrew.SetInfo(SaveLoadSystem.data.LevelData[i].ScrewsIndexes[_currentScrewIndex], _screwsConfig.Materials[SaveLoadSystem.data.LevelData[i].ScrewsIndexes[_currentScrewIndex]]);
                    }

                    _spawnedBolts[i].PlaceScrew(newScrew);
                }
            }

            _currentScrewIndex++;
        }

        ActivateLevel();
    }

    private int GetLevelsMaxScrewAmount()
    {
        int maxScrews = 0;

        foreach(BoltInfo boltinfo in SaveLoadSystem.data.LevelData)
        {
            if (boltinfo.ScrewsIndexes.Length > maxScrews)
                maxScrews = boltinfo.ScrewsIndexes.Length;
        }

        return maxScrews;
    }

    private void GetNecessaryBoltsCount(List<BoltInfo> levelData) 
    {
        int oneBoltScrews = _levelBolt.ScrewPositionsCount;
        int screwsAmount = 0;

        foreach (BoltInfo boltInfo in levelData)
        {
            screwsAmount += boltInfo.ScrewsIndexes.Length;
        }

        _necesaryBolts = screwsAmount / oneBoltScrews;
    }

    private void OnShuffleBoost()
    {
        if (!_isActive)
            return;

        List<Screw> allScrews = new();

        foreach(Bolt bolt in _spawnedBolts)
        {
            if (bolt.IsComplete)
                continue;

            allScrews.AddRange(bolt.GetSameScrews(_shuffleMaxSameScrews));
        }

        int[] newIndices = new int[allScrews.Count];
        Material[] newMaterials = new Material[allScrews.Count];

        for (int i = 0; i < allScrews.Count; i++)
        {
            newIndices[i] = allScrews[i].Index;
            newMaterials[i] = _screwsConfig.Materials[allScrews[i].Index];
        }

        for (int i = 0; i < newIndices.Length; i++)
        {
            int randomIndex = Random.Range(i, newIndices.Length);
            int tempIndex = newIndices[i];
            newIndices[i] = newIndices[randomIndex];
            newIndices[randomIndex] = tempIndex;
            Material tempMaterial = newMaterials[i];
            newMaterials[i] = newMaterials[randomIndex];
            newMaterials[randomIndex] = tempMaterial;
        }

        for (int i = 0; i < allScrews.Count; i++)
        {
            bool wasSecret = allScrews[i].IsSecret;

            if (wasSecret)
            {
                allScrews[i].SetInfo(newIndices[i], newMaterials[i], _screwsConfig.SecretMaterial);
            }
            else
            {
                allScrews[i].SetInfo(newIndices[i], newMaterials[i]);
            }
        }

        foreach (Bolt bolt in _spawnedBolts)
        {
            if (bolt.IsComplete)
                continue;

            bolt.CheckCompletion();
        }
    }

    private void RearrangeGrid()
    {
        CalculateBoltPositions(_spawnedBolts.Count);

        for (int i = 0; i < _spawnedBolts.Count; i++)
        {
            _spawnedBolts[i].transform.localPosition = _boltPositions[i];
        }
    }

    private void OnAddBoltBoost()
    {
        if (!_isActive)
            return;

        Bolt newBolt = Instantiate(_levelBolt, _boltsPlace);
        _spawnedBolts.Add(newBolt);
        newBolt.SetLevel(this);
        RearrangeGrid();
    }


    private void OnEnable()
    {
        EventBus.OnAddBoltBoost += OnAddBoltBoost;
        EventBus.OnShuffleBoost += OnShuffleBoost;
    }

    private void OnDisable()
    {
        EventBus.OnAddBoltBoost -= OnAddBoltBoost;
        EventBus.OnShuffleBoost -= OnShuffleBoost;
    }
}