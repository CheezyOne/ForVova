using UnityEngine;
using UnityEngine.UI;

public class SoundsButton : MonoBehaviour
{
    [SerializeField] private Sprite _soundsOn;
    [SerializeField] private Sprite _soundsOff;
    [SerializeField] private Image _soundsImage;

    public void Awake()
    {
        SetSprite();
    }

    public void OnSoundsButton()
    {
        SaveLoadSystem.data.SoundsOn = !SaveLoadSystem.data.SoundsOn;
        SaveLoadSystem.Instance.Save();
        SoundsManager.Instance.PlaySound(SoundType.Button);
        SetSprite();
    }

    private void SetSprite()
    {
        if (SaveLoadSystem.data.SoundsOn)
        {
            _soundsImage.sprite = _soundsOn;
        }
        else
        {
            _soundsImage.sprite = _soundsOff;
        }
    }
}