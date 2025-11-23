using UnityEngine;
using UnityEngine.UI;

public class MoneyUI : MonoBehaviour
{
    [SerializeField] private Text _moneyText;

    private void Awake()
    {
        UpdateMoneyText();
    }

    private void UpdateMoneyText()
    {
        _moneyText.text = SaveLoadSystem.data.Money.ToString();
    }

    private void OnEnable()
    {
        EventBus.OnMoneyChanged += UpdateMoneyText;
    }

    private void OnDisable()
    {
        EventBus.OnMoneyChanged -= UpdateMoneyText;
    }
}