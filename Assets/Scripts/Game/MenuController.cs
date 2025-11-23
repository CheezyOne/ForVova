using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] private ShopWindow _shopWindow;
    [SerializeField] private GameObject _infiniteModeBlock;
    [SerializeField] private GameObject _infiniteModeInfo;
    [SerializeField] private Text _infiniteModeScoreText;
    [SerializeField] private Text _levelsText;

    private const int INFINITE_MODE_REQUIEREMENT = 3;

    private void Awake()
    {
        if (SaveLoadSystem.data.CurrentLevel >= INFINITE_MODE_REQUIEREMENT)
        {
            _infiniteModeBlock.SetActive(false);
            _infiniteModeInfo.SetActive(true);
            _infiniteModeScoreText.text = SaveLoadSystem.data.HighScore.ToString();
        }
        else
        {
            _infiniteModeBlock.SetActive(true);
            _infiniteModeInfo.SetActive(false);
        }

        _levelsText.text = LanguageSystem.Instance.GetTranslatedText(SharedConstsHolder.LEVEL_KEY) + " " + (SaveLoadSystem.data.CurrentLevel + 1);
    }

    public void OnShopButton()
    {
        SoundsManager.Instance.PlaySound(SoundType.Button);
        WindowsManager.Instance.OpenWindow(_shopWindow);
    }

    public void OnPlayLevelsButton()
    {
        SoundsManager.Instance.PlaySound(SoundType.Button);
        LevelsManager.Instance.SetGameMode(GameMode.Levels);
        LoadGameScene();
    }

    public void OnPlayInfiniteButton()
    {
        SoundsManager.Instance.PlaySound(SoundType.Button);
        LevelsManager.Instance.SetGameMode(GameMode.Infinite);
        LoadGameScene();
    }

    private void LoadGameScene()
    {
        SceneManager.LoadScene(SharedConstsHolder.GAME_SCENE_NAME);
    }
}