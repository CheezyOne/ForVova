using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections;

public class AdsManager : SingletonDontDestroyOnLoad<AdsManager>
{
    [DllImport("__Internal")]
    private static extern void ShowInterAd();

    [DllImport("__Internal")]
    private static extern void ShowRVAd();

    private const float INTER_AD_DEFAULT_TIME = 33f;
    private const float AFTER_RV_TIME = 20f;

    private bool canShowFullscreenAd = false;

    private Coroutine _resetTimerCor;
    private RewardType _currentRewardType;
    private float interAdTimer;

#if UNITY_EDITOR
    [SerializeField] private float _adInEditorTime;
    [SerializeField] private GameObject _editorAdPanel;
#endif

    public static bool IsAdOpened;

    private void Start()
    {
        IsAdOpened = false;
        ResetTimer();
    }

    public void ShowRewarded(RewardType reward)
    {
        _currentRewardType = reward;

#if UNITY_EDITOR
        StartCoroutine(SimulateRewardedAd());
        return;
#endif

        ShowRVAd();
    }

    public void ShowInter()
    {
        if (!canShowFullscreenAd)
            return;

#if UNITY_EDITOR
        StartCoroutine(SimulateInterAd());
        return;
#endif

        ShowInterAd();
    }

    private void ResetTimer()
    {
        if (_resetTimerCor != null)
            StopCoroutine(_resetTimerCor);

        interAdTimer = INTER_AD_DEFAULT_TIME;
        _resetTimerCor = StartCoroutine(ResetTimerIenum());
    }

    private void ResetTimerAfterRV()
    {
        if (interAdTimer >= AFTER_RV_TIME)
            return;

        if (_resetTimerCor != null)
            StopCoroutine(_resetTimerCor);

        interAdTimer = AFTER_RV_TIME;
        _resetTimerCor = StartCoroutine(ResetTimerIenum());
    }

    IEnumerator ResetTimerIenum()
    {
        canShowFullscreenAd = false;
        Debug.Log("CANNOT SHOW AD");

        while (interAdTimer > 0)
        {
            interAdTimer -= Time.deltaTime;
            yield return null;
        }

        canShowFullscreenAd = true;
        Debug.Log("CAN SHOW AD");
    }

#if UNITY_EDITOR
    private IEnumerator SimulateInterAd()
    {
        Debug.Log("Showing inter ad");
        OnInterAdOpen();
        GameObject adPanel = Instantiate(_editorAdPanel, transform);
        yield return new WaitForSecondsRealtime(_adInEditorTime);
        OnInterAdClose();
        Destroy(adPanel);
        Debug.Log("Inter ad shown");
    }

    private IEnumerator SimulateRewardedAd()
    {
        Debug.Log("Showing rewarded ad");
        OnRVOpen();
        GameObject adPanel = Instantiate(_editorAdPanel, transform);
        yield return new WaitForSecondsRealtime(_adInEditorTime);
        OnRVReward();
        OnRVClose();
        Destroy(adPanel);
        Debug.Log("Rewarded ad shown");
    }
#endif

    private void GetReward()
    {
        ResetTimerAfterRV();

        switch (_currentRewardType)
        {
            case RewardType.ContinuePlaying:
                EventBus.OnContinuePlayingReward?.Invoke();
                break;

            case RewardType.Money:
                EventBus.OnMoneyReward?.Invoke();
                break;
        }
    }

    public void OnInterAdOpen()
    {
        IsAdOpened = true;
        Time.timeScale = 0;
    }

    public void OnInterAdClose()
    {
        Time.timeScale = 1;
        ResetTimer();
        IsAdOpened = false;
    }

    public void OnRVOpen()
    {
        IsAdOpened = true;
        Time.timeScale = 0;
    }

    public void OnRVReward()
    {
        GetReward();
    }

    public void OnRVClose()
    {
        Time.timeScale = 1;
        IsAdOpened = false;
    }

    public static bool IsWebGL()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
            return true;
#endif
        return false;
    }
}

public enum RewardType
{
    ContinuePlaying,
    Money,
}