using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayManager : MonoBehaviour
{
    [SerializeField] private ShopWindow _shopWindow;

    public void OnExitButton()
    {
        SoundsManager.Instance.PlaySound(SoundType.Button);
        SceneManager.LoadScene(SharedConstsHolder.MENU_SCENE_NAME);
    }

    public void OnShopButton()
    {
        SoundsManager.Instance.PlaySound(SoundType.Button);
        WindowsManager.Instance.OpenWindow(_shopWindow);
    }
}