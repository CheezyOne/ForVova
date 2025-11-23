using UnityEngine;

public class ScoreManager : Singleton<ScoreManager>
{
    [SerializeField] private int _packScore;

    private int _score;

    public int Score => _score;

    public void AddBoltScore()
    {
        if (WindowsManager.Instance.IsOpened(typeof(LoseWindow)) || LevelsManager.Instance.GameMode != GameMode.Infinite)
            return;

        _score += _packScore;
        LevelUIInfoManager.Instance.UpdateText();
    }

    private void OnBoltCompleted(Bolt bolt)
    {
        AddBoltScore();
    }

    private void OnLevelCompleted()
    {
        if (WindowsManager.Instance.IsOpened(typeof(LoseWindow)) || LevelsManager.Instance.GameMode != GameMode.Infinite)
            return;

        if (_score > SaveLoadSystem.data.HighScore)
        {
            SaveLoadSystem.data.HighScore = _score;
            SaveLoadSystem.Instance.Save();
        }
    }

    private void OnEnable()
    {
        EventBus.OnBoltCompleted += OnBoltCompleted;
        EventBus.OnLevelCompleted += OnLevelCompleted;
    }

    private void OnDisable()
    {
        EventBus.OnBoltCompleted -= OnBoltCompleted;
        EventBus.OnLevelCompleted -= OnLevelCompleted;
    }
}