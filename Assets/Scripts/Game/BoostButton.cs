using UnityEngine;
using UnityEngine.UI;

public class BoostButton : MonoBehaviour
{
    [SerializeField] private bool _deactivateAfterUse;
    [SerializeField] private int _price;
    [SerializeField] private Text _priceText;
    [SerializeField] private BoostType _boostType;
    [SerializeField] private Button _button;

    private bool _isDeactivated;

    public bool DeactivateAfterUse => _deactivateAfterUse;
    public int Price => _price;
    public BoostType BoostType => _boostType;
    public Button Button => _button;
    public bool IsDeactivated => _isDeactivated;

    private void Awake()
    {
        _priceText.text = _price.ToString();
    }

    public void ChangeTextColor(Color color)
    {
        _priceText.color = color; 
    }

    public void Deactivate()
    {
        _isDeactivated = true;
        _button.interactable = false;
    }

    public void Activate()
    {
        _isDeactivated = false;
        _button.interactable = true;
    }
}