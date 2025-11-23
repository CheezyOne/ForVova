using UnityEngine;

public class VictoryManager : MonoBehaviour
{
    [SerializeField] private VictoryWindow _victoryWindow;

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
            LevelVictory();
    }
#endif

    private void LevelVictory()
    {
        SoundsManager.Instance.PlaySound(SoundType.Victory);
        SaveLoadSystem.data.CurrentLevel++;
        WindowsManager.Instance.CloseAllWindows();
        WindowsManager.Instance.OpenWindow(_victoryWindow);
        LevelsManager.Instance.GenerateLevelData();
    }

    private void OnEnable()
    {
        if (LevelsManager.Instance.GameMode == GameMode.Levels)
            EventBus.OnLevelCompleted += LevelVictory;
    }

    private void OnDisable()
    {
        if (LevelsManager.Instance == null)
            return;

        if (LevelsManager.Instance.GameMode == GameMode.Levels)
            EventBus.OnLevelCompleted -= LevelVictory;
    }
}