using System;
using System.Linq;
using Gley.Localization;
using Michsky.MUIP;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(HorizontalSelector))]
public class UISelectorProvider : MonoBehaviour
{
    [SerializeField] private HorizontalSelector _horizontalSelector;
    [SerializeField] private LanguageSelection[] _languageSelections;

    private void Awake()
    {
        Initialize();
        PopulateSelector();
        SelectCurrentLanguage();
    }

    private void OnEnable()
    {
        UpdateSelectorTexts();
    }

    private void Initialize()
    {
        _horizontalSelector ??= GetComponent<HorizontalSelector>();

        InitializeLanguageSelections();

        _horizontalSelector.items.Clear();
    }

    private void InitializeLanguageSelections()
    {
        var supportedLanguages = GetSupportedLanguages();
        _languageSelections = new LanguageSelection[supportedLanguages.Length];

        for (int i = 0; i < supportedLanguages.Length; i++)
        {
            _languageSelections[i] = new LanguageSelection
            {
                language = supportedLanguages[i],
                translationName = GetDefaultWordIDForLanguage(supportedLanguages[i])
            };
        }
    }

    private void PopulateSelector()
    {
        if (_horizontalSelector == null || _languageSelections == null)
            return;

        _horizontalSelector.items.Clear();

        foreach (var selection in _languageSelections)
        {
            _horizontalSelector.CreateNewItem(GetLocalizedText(selection.translationName));
        }

        _horizontalSelector.UpdateUI();
    }

    private void UpdateSelectorTexts()
    {
        if (_horizontalSelector == null || _languageSelections == null)
            return;

        for (int i = 0; i < Mathf.Min(_languageSelections.Length, _horizontalSelector.items.Count); i++)
        {
            _horizontalSelector.items[i].itemTitle = GetLocalizedText(_languageSelections[i].translationName);
        }

        _horizontalSelector.UpdateUI();
    }

    private void SelectCurrentLanguage()
    {
        if (_horizontalSelector == null || _languageSelections == null)
            return;

        var currentLanguage = API.GetCurrentLanguage();
        for (int i = 0; i < _languageSelections.Length; i++)
        {
            if (_languageSelections[i].language == currentLanguage)
            {
                _horizontalSelector.index = i;
                _horizontalSelector.UpdateUI();
                break;
            }
        }
    }

    private string GetLocalizedText(WordIDs wordID)
    {
        return API.GetText(wordID);
    }

    private static SupportedLanguages[] GetSupportedLanguages()
    {
        return Enum.GetValues(typeof(SupportedLanguages)).Cast<SupportedLanguages>().ToArray();
    }

    private WordIDs GetDefaultWordIDForLanguage(SupportedLanguages language)
    {
        return language switch
        {
            SupportedLanguages.English => WordIDs.IDLanguageEnglish,
            SupportedLanguages.Russian => WordIDs.IDLanguageRussian,
            _ => WordIDs.IDLanguageEnglish
        };
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_horizontalSelector == null)
        {
            _horizontalSelector = GetComponent<HorizontalSelector>();
        }

        if (_languageSelections == null || _languageSelections.Length == 0)
        {
            var supportedLanguages = GetSupportedLanguages();
            _languageSelections = new LanguageSelection[supportedLanguages.Length];

            for (int i = 0; i < supportedLanguages.Length; i++)
            {
                _languageSelections[i] = new LanguageSelection
                {
                    language = supportedLanguages[i],
                    translationName = GetDefaultWordIDForLanguage(supportedLanguages[i])
                };
            }
        }

        if (_horizontalSelector != null && Application.isEditor && !Application.isPlaying)
        {
            _horizontalSelector.items.Clear();
            foreach (var selection in _languageSelections)
            {
                var item = new HorizontalSelector.Item();
                item.itemTitle = selection.translationName.ToString();
                _horizontalSelector.items.Add(item);
            }
            //   _horizontalSelector.UpdateUI();
        }
    }
#endif

    public void AddListener(UnityAction<int> action)
    {

        _horizontalSelector?.onValueChanged?.AddListener(action);
    }

    public void RemoveListener(UnityAction<int> action)
    {
        _horizontalSelector?.onValueChanged?.RemoveListener(action);
    }

    public SupportedLanguages GetSelectedLanguage()
    {
        if (_horizontalSelector != null &&
            _languageSelections != null &&
            _horizontalSelector.index >= 0 &&
            _horizontalSelector.index < _languageSelections.Length)
        {
            return _languageSelections[_horizontalSelector.index].language;
        }

        return API.GetCurrentLanguage();
    }

    [Serializable]
    public struct LanguageSelection
    {
        public SupportedLanguages language;
        public WordIDs translationName;
    }
}
