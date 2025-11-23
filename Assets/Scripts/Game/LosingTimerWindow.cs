using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LosingTimerWindow : BaseWindow
{
    [SerializeField] private Image _adImage;
    [SerializeField] private Text _adTimerText;
    [SerializeField] private GameObject _noThanksButton;
    [SerializeField] private float _noThanksTime;
    [SerializeField] private float _timerDuration = 5f;

    public override void Init()
    {
        StartCoroutine(AdTimer());
    }

    public void OnDeclineButton()
    {
        SoundsManager.Instance.PlaySound(SoundType.Button);
        LoseManager.Instance.Lose();
    }

    public void OnReviveButton()
    {
        AdsManager.Instance.ShowRewarded(RewardType.ContinuePlaying);
    }

    private IEnumerator AdTimer()
    {
        float adTimer = _timerDuration;
        float noThxTime = _timerDuration - _noThanksTime;

        while (adTimer > 0)
        {
            adTimer -= Time.deltaTime;
            _adTimerText.text = Mathf.CeilToInt(adTimer).ToString();
            _adImage.fillAmount = adTimer / _timerDuration;

            if (!_noThanksButton.activeSelf && adTimer <= noThxTime)
                _noThanksButton.SetActive(true);

            yield return null;
        }

        LoseManager.Instance.Lose();
    }

    private void OnContinuePlayingReward()
    {
        Close();
    }

    private void OnEnable()
    {
        EventBus.OnContinuePlayingReward += OnContinuePlayingReward;
    }

    private void OnDisable()
    {
        EventBus.OnContinuePlayingReward -= OnContinuePlayingReward;
    }
}