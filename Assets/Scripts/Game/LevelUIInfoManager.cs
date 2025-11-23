using UnityEngine;
using UnityEngine.UI;

public class LevelUIInfoManager : Singleton<LevelUIInfoManager>
{
    [SerializeField] private float _boltTimeDelta;
    [SerializeField] private float _startingTime;
    [SerializeField] private float _boltTime;
    [SerializeField] private float _boltScore;
    [SerializeField] private float _reviveTime;
    [SerializeField] private Text _levelText;
    [SerializeField] private Text _levelSpecific;
    [SerializeField] private AddedTime _addedTime;
    [SerializeField] private Transform _addedTimeSpawnPoint;

    private const string SKYSCRAPER_LEVEL_KEY = "skyscraper_level";
    private const string SHORT_LEVEL_KEY = "short_level";
    private const string TALL_LEVEL_KEY = "tall_level";
    private const string SCORE_KEY = "score";

    private int _completeBoltsCounter;

    protected override void Awake()
    {
        base.Awake();
        UpdateText();

        if (LevelsManager.Instance.GameMode == GameMode.Levels)
        {
            SetLevelsSpecificText();
        }
        else
        {
            _levelSpecific.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (LevelsManager.Instance.GameMode != GameMode.Infinite || _startingTime <= 0)
            return;

        _startingTime -= Time.deltaTime;

        if(_startingTime<=0)
        {
            _startingTime = 0;
            LoseManager.Instance.Lose();
        }

        _levelSpecific.text = FormatTime(_startingTime);
    }

    public void UpdateText()
    {
        if (LevelsManager.Instance.GameMode == GameMode.Levels)
        {
            _levelText.text = LanguageSystem.Instance.GetTranslatedText(SharedConstsHolder.LEVEL_KEY) + " " + (SaveLoadSystem.data.CurrentLevel + 1);
        }
        else
        {
            _levelText.text = LanguageSystem.Instance.GetTranslatedText(SCORE_KEY) + ScoreManager.Instance.Score.ToString();
        }
    }

    private void OnReviveBoost()
    {
        if (LevelsManager.Instance.GameMode == GameMode.Levels)
            return;

        AddTime(_reviveTime);
    }

    private void OnBoltCompleted(Bolt bolt)
    {
        if (LevelsManager.Instance.GameMode == GameMode.Levels || WindowsManager.Instance.IsOpened(typeof(LosingTimerWindow)) || WindowsManager.Instance.IsOpened(typeof(LoseWindow)))
            return;

        _completeBoltsCounter++;
        AddTime(_boltTime + (_boltTimeDelta*_completeBoltsCounter));
        UpdateText();
    }

    private void AddTime(float time)
    {
        _startingTime += time;
        Instantiate(_addedTime, _addedTimeSpawnPoint.position, Quaternion.identity, _levelSpecific.transform).Init((int)time);
    }

    private void SetLevelsSpecificText()
    {
        if (!LevelsManager.Instance.IsDifficultLevel)
            return;

        switch (SaveLoadSystem.data.LevelType)
        {
            case DifficultLevelType.Short:
                {
                    _levelSpecific.text = LanguageSystem.Instance.GetTranslatedText(SHORT_LEVEL_KEY);
                    _levelSpecific.gameObject.SetActive(true);
                    break;
                }
            case DifficultLevelType.Tall:
                {
                    _levelSpecific.text = LanguageSystem.Instance.GetTranslatedText(TALL_LEVEL_KEY);
                    _levelSpecific.gameObject.SetActive(true);
                    break;
                }
            case DifficultLevelType.Skyscraper:
                {
                    _levelSpecific.text = LanguageSystem.Instance.GetTranslatedText(SKYSCRAPER_LEVEL_KEY);
                    _levelSpecific.gameObject.SetActive(true);
                    break;
                }
            default:
                {
                    _levelSpecific.gameObject.SetActive(false);
                    break;
                }
        }
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        minutes = Mathf.Max(0, minutes);
        seconds = Mathf.Max(0, seconds);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void OnEnable()
    {
        EventBus.OnBoltCompleted += OnBoltCompleted;
        EventBus.OnContinuePlayingReward += OnReviveBoost;
    }

    private void OnDisable()
    {
        EventBus.OnBoltCompleted -= OnBoltCompleted;
        EventBus.OnContinuePlayingReward -= OnReviveBoost;
    }
}