using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = nameof(LanguageConfig), menuName = "Avlerm/Configs/LanguageConfig")]
public class LanguageConfig : ScriptableObject
{
    [SerializeField] private AllLanguages _targetLanguage;
    [SerializeField] private LanguageStruct[] _languages;
    [SerializeField] private ArrayLanguageStruct[] _arrayLanguages;

    private List<AllLanguages> _languagesList = new List<AllLanguages>() { AllLanguages.ru, AllLanguages.en };
    private List<string> _russianLanguages = new List<string>() { "ab", "be", "kk", "tg", "tt", "uk", "uz" };

    public AllLanguages TargetLanguage => _targetLanguage;
    public LanguageStruct[] Languages => _languages;
    public ArrayLanguageStruct[] ArrayLanguages => _arrayLanguages;
    public List<AllLanguages> LanguagesList => _languagesList;
    public List<string> RussianLanguages => _russianLanguages;
}

[Serializable]
public struct LanguageStruct
{
    public string Key;
    public TranslateStruct[] Translates;
}

[Serializable]
public struct TranslateStruct
{
    public AllLanguages SystemLanguage;
    [TextArea] public string text;

    public TranslateStruct(AllLanguages lang, string txt)
    {
        SystemLanguage = lang;
        text = txt;
    }
}

[Serializable]
public struct ArrayLanguageStruct
{
    public string Key;
    public ArrayTranslateStruct[] ArrayTranslates;
}

[Serializable]
public struct ArrayTranslateStruct
{
    public AllLanguages SystemLanguage;
    public string[] texts;
}

public enum AllLanguages
{
    ru,
    en
}