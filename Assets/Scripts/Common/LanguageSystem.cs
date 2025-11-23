using System.Collections;
using UnityEngine;
using System.Linq;
using System.Runtime.InteropServices;
using System;
using Random = UnityEngine.Random;

public class LanguageSystem : SingletonDontDestroyOnLoad<LanguageSystem>
{
    [DllImport("__Internal")]
    private static extern string GetLang();

    [DllImport("__Internal")]
    private static extern bool PlayerInited();

    [SerializeField] private LanguageConfig _config;

    private string _currentLang = DEFAULT_LANGUAGE;
    private Coroutine _setLangCor;

    public string GetGameLanguage
    {
        get
        {
            if (AdsManager.IsWebGL() || _config == null)
                return _currentLang;

            return _config.TargetLanguage.ToString();
        }
    }

    public static Action OnLanguageSetted;

    public LanguageConfig Config => _config;

    public const string DEFAULT_LANGUAGE = "ru";

    private void Start()
    {
        SetCurrentLang();
    }

    private bool CurrentLangIsInList()
    {
        bool contain = false;
        for (int i = 0; i < _config.LanguagesList.Count; i++)
        {
            if (_config.LanguagesList[i].ToString() == _currentLang)
            {
                contain = true;
                break;
            }
        }
        return contain;
    }

    private void SetCurrentLang()
    {
        if (_setLangCor != null || !AdsManager.IsWebGL())
            return;

        _setLangCor = StartCoroutine(SetCurrentLangIenum());
    }

    private IEnumerator SetCurrentLangIenum()
    {
        while (!PlayerInited())
        {
            yield return null;
        }

        _currentLang = GetLang();

        if (!CurrentLangIsInList())
        {
            if (_config.RussianLanguages.Contains(_currentLang))
            {
                _currentLang = "ru";
            }
            else
            {
                _currentLang = "en";
            }
        }

        OnLanguageSetted?.Invoke();
    }

    public string GetTranslatedText(string key)
    {
        string translatedTxt = "";

        LanguageStruct language = _config.Languages.FirstOrDefault(x => x.Key.Equals(key));

        if (language.Equals(default))
        {
            return "";
        }

        TranslateStruct translate = language.Translates
            .FirstOrDefault(x => x.SystemLanguage.ToString().Equals(GetGameLanguage));

        if (translate.Equals(default))
            return "";

        translatedTxt = translate.text;

        return translatedTxt;
    }

    public string GetTranslatedTextFromArrayByID(string key, int id)
    {
        string translatedTxt = "";

        ArrayLanguageStruct language = _config.ArrayLanguages.FirstOrDefault(x => x.Key.Equals(key));
        if (language.Equals(default))
        {
            return "";
        }

        ArrayTranslateStruct translate = language.ArrayTranslates
            .FirstOrDefault(x => x.SystemLanguage.ToString().Equals(GetGameLanguage));

        if (translate.Equals(default))
            return "";

        translatedTxt = translate.texts[id];

        return translatedTxt;
    }

    public string GetRandomTranslatedText(string key)
    {
        string translatedTxt = "";

        ArrayLanguageStruct language = _config.ArrayLanguages.FirstOrDefault(x => x.Key.Equals(key));
        if (language.Equals(default))
        {
            return "";
        }

        ArrayTranslateStruct translate = language.ArrayTranslates
            .FirstOrDefault(x => x.SystemLanguage.ToString().Equals(GetGameLanguage));

        if (translate.Equals(default))
            return "";

        translatedTxt = translate.texts[Random.Range(0, translate.texts.Length)];

        return translatedTxt;
    }
}