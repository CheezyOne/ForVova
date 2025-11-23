using UnityEngine;

public class MoneyManager : SingletonDontDestroyOnLoad<MoneyManager>
{
#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
            AddMoney(100);
    }
#endif

    public bool CanBuy(int price)
    {
        return price <= SaveLoadSystem.data.Money;
    }

    public void AddMoney(int money, bool save = true)
    {
        SaveLoadSystem.data.Money += money;

        if(save)
            SaveMoney();
    }

    public void SpendMoney(int price)
    {
        SaveLoadSystem.data.Money -= price;
        SaveMoney();
    }

    private void SaveMoney()
    {
        EventBus.OnMoneyChanged?.Invoke();
        SaveLoadSystem.Instance.Save();
    }
}