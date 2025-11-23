using System;
using UnityEngine;

public class BoostsManager : Singleton<BoostsManager>
{
    [SerializeField] private ShopWindow _shopWindow;
    [SerializeField] private Color _isAvailableColor;
    [SerializeField] private Color _notAvailableColor;
    [SerializeField] private BoostButton[] _boostButtons;
    [SerializeField] private InputManager _inputManager;

    protected override void Awake()
    {
        base.Awake();
        UpdateTextsColor();
        DisableButtons();

        if(SaveLoadSystem.data.CurrentLevel == 0)
        {
            foreach (BoostButton boostButton in _boostButtons)
            {
                boostButton.Deactivate();
            }
        }
    }

    public void CheckReturnButton(bool deactivate = true)
    {
        foreach (BoostButton boostButton in _boostButtons)
        {
            if (boostButton.BoostType != BoostType.ReturnScrew)
                return;

            if (!_inputManager.CanReturnScrews())
            {
                if (!deactivate)
                    return;

                boostButton.Deactivate();
            }
            else
            {
                boostButton.Activate();
            }
        }
    }

    public void DisableButtons()
    {
        for (int i = 0; i < _boostButtons.Length; i++)
        {
            _boostButtons[i].Button.interactable = false;
        }
    }

    public void EnableButtons()
    {
        for (int i = 0; i < _boostButtons.Length; i++)
        {
            if (_boostButtons[i].IsDeactivated)
                continue;

            _boostButtons[i].Button.interactable = true;
        }
    }

    public void OnBoostButton(int index)
    {
        SoundsManager.Instance.PlaySound(SoundType.Button);
        Action boostAction = null;

        if (_boostButtons[index].BoostType == BoostType.ReturnScrew && !_inputManager.CanReturnScrews())
            return;

        switch (_boostButtons[index].BoostType)
        {
            case BoostType.ReturnScrew:
                {
                    boostAction = EventBus.OnScrewReturnBoost;
                    break;
                }
            case BoostType.AddBolt:
                {
                    boostAction = EventBus.OnAddBoltBoost;
                    break;
                }
            case BoostType.Shuffle:
                {
                    boostAction = EventBus.OnShuffleBoost;
                    break;
                }
            default:
                {
                    break;
                }
        }

        HandleBoost(boostAction, index, _boostButtons[index].DeactivateAfterUse);
    }

    private void UpdateTextsColor()
    {
        for (int i = 0; i < _boostButtons.Length; i++)
        {
            _boostButtons[i].ChangeTextColor(MoneyManager.Instance.CanBuy(_boostButtons[i].Price) ? _isAvailableColor : _notAvailableColor);
        }
    }

    private void HandleBoost(Action boostAction, int index, bool deactivateAfterUse = false)
    {
        if (!MoneyManager.Instance.CanBuy(_boostButtons[index].Price))
        {
            WindowsManager.Instance.OpenWindow(_shopWindow);
            return;
        }

        boostAction?.Invoke();

        if (deactivateAfterUse)
            _boostButtons[index].Deactivate();

        MoneyManager.Instance.SpendMoney(_boostButtons[index].Price);
    }

    private void ActivateButtons()
    {
        foreach(BoostButton button in _boostButtons)
        {
            button.Activate();
        }
    }

    private void OnScrewReturnBoost()
    {
        CheckReturnButton();
    }

    private void OnLevelGenerated()
    {
        CheckReturnButton();
    }

    private void OnBoltCompleted(Bolt bolt)
    {
        CheckReturnButton();
    }

    private void OnEnable()
    {
        EventBus.OnMoneyChanged += UpdateTextsColor;
        EventBus.OnLevelCompleted += ActivateButtons;
        EventBus.OnLevelCompleted += DisableButtons;
        EventBus.OnScrewReturnBoost += OnScrewReturnBoost;
        EventBus.OnLevelGenerated += OnLevelGenerated;
        EventBus.OnBoltCompleted += OnBoltCompleted;
    }

    private void OnDisable()
    {
        EventBus.OnMoneyChanged -= UpdateTextsColor;
        EventBus.OnLevelCompleted -= ActivateButtons;
        EventBus.OnLevelCompleted -= DisableButtons;
        EventBus.OnScrewReturnBoost -= OnScrewReturnBoost;
        EventBus.OnLevelGenerated -= OnLevelGenerated;
        EventBus.OnBoltCompleted -= OnBoltCompleted;
    }
}

public enum BoostType
{
    ReturnScrew,
    AddBolt,
    Shuffle,
}