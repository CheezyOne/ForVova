using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VictoryWindow : BaseWindow
{
    [SerializeField] private Text _levelNumberText;
    [SerializeField] private Text _moneyAddedText;
    [SerializeField] private int _moneyAddedForLevel;

    private const string LEVEL_COMPLETED_KEY = "level_completed";

    public override void Init()
    {
        base.Init();
        _levelNumberText.text = LanguageSystem.Instance.GetTranslatedTextFromArrayByID(LEVEL_COMPLETED_KEY, 0) + " " +
            SaveLoadSystem.data.CurrentLevel + " " +
            LanguageSystem.Instance.GetTranslatedTextFromArrayByID(LEVEL_COMPLETED_KEY, 1);
        _moneyAddedText.text = "+" + _moneyAddedForLevel;
        MoneyManager.Instance.AddMoney(_moneyAddedForLevel, false);
    }

    public void OnMenuButton()
    {
        SoundsManager.Instance.PlaySound(SoundType.Button);
        SceneManager.LoadScene(SharedConstsHolder.MENU_SCENE_NAME);
    }

    public void OnNextLevelButton()
    {
        SoundsManager.Instance.PlaySound(SoundType.Button);
        SceneManager.LoadScene(SharedConstsHolder.GAME_SCENE_NAME);
    }
}