using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class LanguageSetter : MonoBehaviour
{
    [SerializeField]
    private TranslateStruct[] translates = new TranslateStruct[2]
    {
        new TranslateStruct(AllLanguages.ru,""),
        new TranslateStruct(AllLanguages.en,"")
    };
    [SerializeField] private Text text;

    private void OnEnable()
    {
        LanguageSystem.OnLanguageSetted += SetTranslate;
    }

    private void OnDisable()
    {
        LanguageSystem.OnLanguageSetted -= SetTranslate;
    }

    private void Start()
    {
        SetTranslate();
    }

    private void SetTranslate()
    {
        if (text == null || translates == null || translates.Length <= 0)
            return;

        string targetLanguage = LanguageSystem.DEFAULT_LANGUAGE;


#if UNITY_EDITOR
        targetLanguage = Resources.Load<LanguageConfig>(nameof(LanguageConfig)).TargetLanguage.ToString();
#else
        targetLanguage = LanguageSystem.Instance?.GetGameLanguage ?? LanguageSystem.DEFAULT_LANGUAGE;
#endif

        string translatedTxt = "";

        TranslateStruct translate = translates.FirstOrDefault(x => x.SystemLanguage.ToString().Equals(targetLanguage));

        if (translate.Equals(default))
            return;

        translatedTxt = translate.text;

        if (text != null)
            text.text = translatedTxt;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (text == null)
        {
            text = GetComponent<Text>();
        }

        SetTranslate();
    }
#endif
}