using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class SaveLoadSystem : SingletonDontDestroyOnLoad<SaveLoadSystem>
{
    [DllImport("__Internal")]
    private static extern void SaveExtern(string data);

    [DllImport("__Internal")]
    private static extern void LoadExtern();

    [DllImport("__Internal")]
    private static extern bool PlayerInited();

    [DllImport("__Internal")]
    private static extern void CallApiReady();

    public static PlayerData data;

    public static Action OnLoaded;

    private const string PROGRESS_KEY = "progress";

    [SerializeField] private float _waitForInit = 9f;

#if UNITY_EDITOR
    [SerializeField] private bool _lateLoadTest;
    [SerializeField] private float _lateLoadTime;
#endif
    private bool _apiReadyCalled;

    private void Start()
    {
        if (AdsManager.IsWebGL())
        {
            StartCoroutine(LoadIenum());
        }
        else
        {
#if UNITY_EDITOR
            if (_lateLoadTest)
            {
                StartCoroutine(LateLoadRoutine());
                return;
            }
#endif
            Load();
        }
    }

#if UNITY_EDITOR
    private IEnumerator LateLoadRoutine()
    {
        yield return new WaitForSeconds(_lateLoadTime);
        Load();
    }
#endif
    public void Save()
    {
        data.SavesCount++;
        string jsonString = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(PROGRESS_KEY, jsonString);

        if (AdsManager.IsWebGL())
        {
            SaveExtern(jsonString);
        }
    }

    private IEnumerator LoadIenum()
    {
        float timer = _waitForInit;
        Debug.Log("WAIT FOR PLAYER INIT_" + timer.ToString());
        while (!PlayerInited() && timer > 0)
        {
            timer -= Time.unscaledDeltaTime;
            yield return null;
        }
        Load();
        Debug.Log("PLAYER INITED IN UNITY_" + timer.ToString());
    }

    private void Load()
    {
        if (PlayerPrefs.HasKey(PROGRESS_KEY))
        {
            string s = PlayerPrefs.GetString(PROGRESS_KEY);
            data = JsonUtility.FromJson<PlayerData>(s);
        }
        else
        {
            Debug.Log("NEW_PLAYER_LOCAL");
            data = new PlayerData();
        }

        if (AdsManager.IsWebGL())
        {
            LoadExtern();
        }
        else
        {
            SavesLoaded();
        }
    }

    public void SetExternPlayerData(string strData)
    {
        PlayerData externData = null;

        if (strData == "{}" || string.IsNullOrEmpty(strData))
        {
            Debug.Log("NEW_PLAYER_EXTERN");
            externData = new PlayerData();
        }
        else
            externData = JsonUtility.FromJson<PlayerData>(strData);

        if (externData != null && externData.SavesCount > data.SavesCount)
        {
            data = externData;
            Debug.Log("CHANGED DATA TO EXTERN.");
        }

        SavesLoaded();
    }

    public void SavesLoaded()
    {
        OnLoaded?.Invoke();

        if (!_apiReadyCalled && AdsManager.IsWebGL())
        {
            _apiReadyCalled = true;
            CallApiReady();
        }
    }
}

[Serializable]
public class PlayerData
{
    public int SavesCount;
    public bool SoundsOn = true;

    public DifficultLevelType LevelType;
    public List<BoltInfo> LevelData;
    public int HighScore;
    public int CurrentLevel;
    public int Money;
}