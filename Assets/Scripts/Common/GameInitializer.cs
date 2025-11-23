using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInitializer : MonoBehaviour
{
    private void OnSavesLoaded()
    {
        SceneManager.LoadScene("Menu");
    }

    protected void OnEnable()
    {
        SaveLoadSystem.OnLoaded += OnSavesLoaded;
    }

    private void OnDisable()
    {
        SaveLoadSystem.OnLoaded -= OnSavesLoaded;
    }
}