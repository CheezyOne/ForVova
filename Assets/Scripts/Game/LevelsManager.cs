using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelsManager : SingletonDontDestroyOnLoad<LevelsManager>
{
    [Header("Levels mode")]
    [SerializeField] private int _difficultLevelFrequency; //Каждые сколько уровней будет усложнённый уровень
    [SerializeField] private int _maximumBolts;//Сколько максимально на уровне заполненных болтов
    [SerializeField] private int _minimumBolts; //Сколько минимально на уровне заполненных болтов
    [SerializeField] private int _addBoltStep = 3; //Каждые сколько уровней увеличиться количество болтов на очередном уровне
    [SerializeField] private int _skyscraperMaximum; //Сколько максимально на небоскрёбном уровне заполненных болтов (отдельная переменная, потому что болты слишком больше, влезает меньше)
    [SerializeField] private DifficultLevelInfo[] _difficultLevelInfos; //Количество гаек на болте в режиме, вероятность именно этого режима (выбирается один из всех каждый раз)

    [Header("Infinite mode")]
    [SerializeField] private int _infiniteDifficultyStep; //Каждые сколько уровней увеличится количество болтов на очередном бесконечном уровне
    [SerializeField] private int _infiniteMinimumBolts; //Со скольки заполненных болтов начинаем бесконечный уровень
    [SerializeField] private int _infiniteMaximumBolts;//Сколько заполненных болтов максимально в бесконечном уровне

    private const int DEFAULT_SCREWS_AMOUNT = 4;
    private int _infiniteLevelsCounter;
    private GameMode _gameMode = GameMode.Levels;
    private bool _isDifficultLevel;

    public const int TUTORIAL_FIRST_BOLT_SCREWS = 1;
    public const int TUTORIAL_SECOND_BOLT_SCREWS = 3;
    public const int TUTORIAL_BOLTS_AMOUNT = 2;

    public GameMode GameMode => _gameMode;
    public bool IsDifficultLevel => _isDifficultLevel;

    protected override void Awake()
    {
        base.Awake();

        if (SaveLoadSystem.data.LevelData == null)
        {
            GenerateLevelData();
        }
        else if ((SaveLoadSystem.data.CurrentLevel + 1) % _difficultLevelFrequency == 0)
        {
            _isDifficultLevel = true;
        }
    }

    public void ResetInfiniteCounter()
    {
        _infiniteLevelsCounter = 0;
    }

    public void GenerateLevelData()
    {
        if ((SaveLoadSystem.data.CurrentLevel + 1) % _difficultLevelFrequency == 0)
        {
            SetDifficultLevel();
        }
        else
        {
            _isDifficultLevel = false;
        }

        LevelData newLevelData = new();
        SaveLoadSystem.data.LevelData = newLevelData.SavedBoltInfos;
        SaveLoadSystem.Instance.Save();
    }

    public int GetScrewsInBolt()// Количество гаек на болте определяется в зависимости от типа уровня (в бесконечном всегда по обычному количеству)
    {
        if (_gameMode == GameMode.Infinite || !_isDifficultLevel)
            return DEFAULT_SCREWS_AMOUNT;

        DifficultLevelType levelType = SaveLoadSystem.data.LevelType;

        foreach (DifficultLevelInfo levelInfo in _difficultLevelInfos)
        {
            if(levelInfo.LevelType == levelType)
                return levelInfo.ScrewsAmount;
        }

        return 0;
    }

    public void SetGameMode(GameMode gameMode)
    {
        _gameMode = gameMode;
    }

    public void SetDifficultLevel() //Случайный усложнённый уровень в зависимости от весов
    {
        _isDifficultLevel = true;
        float totalWeight = 0f;

        foreach (var levelInfo in _difficultLevelInfos)
        {
            totalWeight += levelInfo.Weight;
        }

        if (totalWeight <= 0f)
        {
            int randomIndex = Random.Range(0, _difficultLevelInfos.Length);
            SaveLoadSystem.data.LevelType = _difficultLevelInfos[randomIndex].LevelType;
            return;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var levelInfo in _difficultLevelInfos)
        {
            currentWeight += levelInfo.Weight;

            if (randomValue <= currentWeight)
            {
                SaveLoadSystem.data.LevelType = levelInfo.LevelType;
                return;
            }
        }
    }

    public int GetBoltsAmount()// В зависимости от количества пройденных уровней получаем количество болтов для очередного уровня (и для бесконечного, и для обычного режимов)
    {
        int boltsAmount = 0;

        if (_gameMode == GameMode.Infinite)
        {
            boltsAmount = _infiniteMinimumBolts;
            boltsAmount += _infiniteLevelsCounter / _infiniteDifficultyStep;
            
            if(boltsAmount>_infiniteMaximumBolts)
                boltsAmount = _infiniteMaximumBolts;

            _infiniteLevelsCounter++;
        }
        else 
        {
            boltsAmount = _minimumBolts;
            boltsAmount += SaveLoadSystem.data.CurrentLevel / _addBoltStep;

            if (boltsAmount > _maximumBolts)
                boltsAmount = _maximumBolts;

            if (_isDifficultLevel && SaveLoadSystem.data.LevelType == DifficultLevelType.Skyscraper)
            {
                if(boltsAmount > _skyscraperMaximum)
                    boltsAmount = _skyscraperMaximum;
            }
        }

        return boltsAmount;
    }
}

[Serializable]
public class LevelData
{
    private List<BoltInfo> _savedBoltInfos = new();

    public List<BoltInfo> SavedBoltInfos => _savedBoltInfos;

    public LevelData()
    {
        if (SaveLoadSystem.data.CurrentLevel == 0)
        {
            SetUpFirstTutorialLevel();
        }
        else if (SaveLoadSystem.data.CurrentLevel == 1)
        {
            SetUpSecondTutorialLevel();
        }
        else
        {
            SetUpLevel();
        }
    }

    private void SetUpLevel()
    {
        int boltsAmount = LevelsManager.Instance.GetBoltsAmount();
        int screwsInBolt = LevelsManager.Instance.GetScrewsInBolt();

        for (int i = 0; i < boltsAmount; i++)
        {
            BoltInfo newBoltInfo = new();
            newBoltInfo.ScrewsIndexes = new int[screwsInBolt];

            for (int j = 0; j < newBoltInfo.ScrewsIndexes.Length; j++)
            {
                newBoltInfo.ScrewsIndexes[j] = i;
            }

            _savedBoltInfos.Add(newBoltInfo);
        }

        ShuffleScrewIndexes();
        int emptyBolts = 2;

        for (int i = 0; i < emptyBolts; i++)
        {
            BoltInfo newBoltInfo = new();
            newBoltInfo.ScrewsIndexes = new int[0];
            _savedBoltInfos.Add(newBoltInfo);
        }
    }

    private void SetUpFirstTutorialLevel()
    {
        BoltInfo newBoltInfo = new();
        newBoltInfo.ScrewsIndexes = new int[LevelsManager.TUTORIAL_FIRST_BOLT_SCREWS];

        for (int j = 0; j < newBoltInfo.ScrewsIndexes.Length; j++)
        {
            newBoltInfo.ScrewsIndexes[j] = 0;
        }

        _savedBoltInfos.Add(newBoltInfo);

        newBoltInfo = new();
        newBoltInfo.ScrewsIndexes = new int[LevelsManager.TUTORIAL_SECOND_BOLT_SCREWS];

        for (int j = 0; j < newBoltInfo.ScrewsIndexes.Length; j++)
        {
            newBoltInfo.ScrewsIndexes[j] = 0;
        }
        
        _savedBoltInfos.Add(newBoltInfo);
    }

    private void SetUpSecondTutorialLevel()
    {
        int screwsInBolt = LevelsManager.Instance.GetScrewsInBolt();
        BoltInfo newBoltInfo;

        for (int i = 0; i < LevelsManager.TUTORIAL_BOLTS_AMOUNT; i++)
        {
            newBoltInfo = new();
            newBoltInfo.ScrewsIndexes = new int[screwsInBolt];

            for (int j = 0; j < newBoltInfo.ScrewsIndexes.Length; j++)
            {
                newBoltInfo.ScrewsIndexes[j] = i;
            }

            _savedBoltInfos.Add(newBoltInfo);
        }

        int randomSwitchedScrew = Random.Range(0, _savedBoltInfos[0].ScrewsIndexes.Length);
        int firstBoltIndex = _savedBoltInfos[0].ScrewsIndexes[randomSwitchedScrew];

        for (int i = 0; i < LevelsManager.TUTORIAL_BOLTS_AMOUNT - 1; i++)
        {
            _savedBoltInfos[i].ScrewsIndexes[randomSwitchedScrew] = _savedBoltInfos[i + 1].ScrewsIndexes[randomSwitchedScrew];
        }

        _savedBoltInfos[LevelsManager.TUTORIAL_BOLTS_AMOUNT - 1].ScrewsIndexes[randomSwitchedScrew] = firstBoltIndex;
        newBoltInfo = new();
        newBoltInfo.ScrewsIndexes = new int[0];
        _savedBoltInfos.Add(newBoltInfo);
    }

    private void ShuffleScrewIndexes()
    {
        List<int> allScrews = new List<int>();

        foreach (BoltInfo bolt in _savedBoltInfos)
        {
            allScrews.AddRange(bolt.ScrewsIndexes);
        }

        ShuffleList(allScrews);

        int screwIndex = 0;

        foreach (BoltInfo bolt in _savedBoltInfos)
        {
            for (int i = 0; i < bolt.ScrewsIndexes.Length; i++)
            {
                bolt.ScrewsIndexes[i] = allScrews[screwIndex];
                screwIndex++;
            }
        }
    }

    private void ShuffleList<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}

[Serializable]
public class BoltInfo
{
    public int[] ScrewsIndexes;
}

[Serializable] 
public class DifficultLevelInfo
{
    public DifficultLevelType LevelType;
    public int ScrewsAmount;
    public float Weight;
}

public enum GameMode
{
    Infinite,
    Levels
}

public enum DifficultLevelType
{
    Short,
    Tall,
    Skyscraper,
    Secret,
}