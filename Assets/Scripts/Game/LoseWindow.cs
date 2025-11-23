using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LoseWindow : BaseWindow
{
    [SerializeField] private Transform _restartButton;
    [SerializeField] private Transform _menuButton;
    [SerializeField] private float _buttonsAnimationDelta;
    [SerializeField] private float _buttonsAnimationTime;
    [SerializeField] private float _buttonsAnimationScale;
    [SerializeField] private Text _currentScoreText;
    [SerializeField] private Text _highScoreText;
    [SerializeField] private Text _newHighScoreText;

    private const float COUNTING_FRAMES = 75f;

    private int _scoreCounter;
    private int _currentScore;
    private int _delta;

    public override void Init()
    {
        _scoreCounter = 0;
        _currentScore = ScoreManager.Instance.Score;
        _highScoreText.text = SaveLoadSystem.data.HighScore.ToString();
        _delta = Mathf.CeilToInt((float)_currentScore / COUNTING_FRAMES);
        StartCountingUp();
    }

    public void OnRestartButton()
    {
        SoundsManager.Instance.PlaySound(SoundType.Button);
        SceneManager.LoadScene(SharedConstsHolder.GAME_SCENE_NAME);
    }

    public void OnMenuButton()
    {
        SoundsManager.Instance.PlaySound(SoundType.Button);
        SceneManager.LoadScene(SharedConstsHolder.MENU_SCENE_NAME);
    }

    private void StartCountingUp()
    {
        StartCoroutine(CountUpScore());
    }

    private IEnumerator CountUpScore()
    {
        while (_scoreCounter + _delta < _currentScore)
        {
            _scoreCounter += _delta;
            _currentScoreText.text = _scoreCounter.ToString();
            yield return null;
        }

        _scoreCounter = _currentScore;
        _currentScoreText.text = _currentScore.ToString();

        if (_currentScore > SaveLoadSystem.data.HighScore)
        {
            _newHighScoreText.gameObject.SetActive(true);
            _highScoreText.text = _currentScore.ToString();
            SaveLoadSystem.data.HighScore = _currentScore;
            SaveLoadSystem.Instance.Save();
        }

        PopupButtons();
    }

    private void PopupButtons()
    {
        _restartButton.DOScale(_buttonsAnimationScale, _buttonsAnimationTime).SetEase(Ease.OutBounce);
        _menuButton.DOScale(_buttonsAnimationScale, _buttonsAnimationTime).SetDelay(_buttonsAnimationDelta).SetEase(Ease.OutBounce);
    }
}