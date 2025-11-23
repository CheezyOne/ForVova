using UnityEngine;
using UnityEngine.UI;

public class ShopWindow : BaseWindow
{
    [SerializeField] private Text _getMoneyText;
    [SerializeField] private int _adMoney;

    private const string GET_MONEY_KEY = "get_money";

    public override void Init()
    {
        base.Init();
        _getMoneyText.text = LanguageSystem.Instance.GetTranslatedTextFromArrayByID(GET_MONEY_KEY, 0) + " " +
            _adMoney + " " +
            LanguageSystem.Instance.GetTranslatedTextFromArrayByID(GET_MONEY_KEY, 1);
    }

    public void OnGetMoneyAdButton()
    {
        AdsManager.Instance.ShowRewarded(RewardType.Money);
    }

    public void OnCloseButton()
    {
        SoundsManager.Instance.PlaySound(SoundType.Button);
        Close();
    }

    private void OnMoneyReward()
    {
        MoneyManager.Instance.AddMoney(_adMoney);
    }

    private void OnEnable()
    {
        EventBus.OnMoneyReward += OnMoneyReward;
    }

    private void OnDisable()
    {
        EventBus.OnMoneyReward -= OnMoneyReward;
    }
}