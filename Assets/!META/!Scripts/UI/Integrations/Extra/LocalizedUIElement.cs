using Gley.Localization;
using UnityEngine;
using System.Collections.Generic;
using System;

public abstract class LocalizedUIElement : MonoBehaviour
{
    [SerializeField] protected List<WordIDs> _wordIDs = new() { WordIDs.NULLID };
    [SerializeField] protected string _separator = " ";
    [SerializeField] protected bool _useMultipleWord = false;

    public bool UseMultipleWord => _useMultipleWord;

    public WordIDs WordID
    {
        get => _wordIDs.Count > 0 ? _wordIDs[0] : default;
        set
        {
            if (_wordIDs.Count == 0)
            {
                _wordIDs.Add(value);
            }
            else
            {
                _wordIDs[0] = value;
            }
        }
    }

    public string WordIDString => WordID.ToString();

    protected virtual void Awake()
    {
        UIUpdateLocalized.OnUpdateLocalized += UpdateLocalization;

        InitializeComponents();
        UpdateLocalization();
    }

    private void OnDisable()
    {
        UIUpdateLocalized.OnUpdateLocalized -= UpdateLocalization;
    }

    protected virtual void OnValidate()
    {
        InitializeComponents();
        UpdateLocalizationEditor();
    }

    public virtual void UpdateLocalization()
    {
        if (_useMultipleWord && _wordIDs.Count > 1)
        {
            SetText(GetLocalizedTextMultiple());
        }
        else if (_wordIDs.Count > 0)
        {
            SetText(GetLocalizedTextSingle());
        }
    }

    public virtual void UpdateLocalizationEditor()
    {
        if (_useMultipleWord && _wordIDs.Count > 1)
        {
            SetText(GetLocalizedTextMultipleEditor());
        }
        else if (_wordIDs.Count > 0)
        {
            SetText(_wordIDs[0].ToString());
        }
    }
    
    protected virtual string GetLocalizedTextSingle(int index = 0)
    {
        return _wordIDs.Count == 0 ? string.Empty : API.GetText(_wordIDs[index]);
    }

    protected virtual string GetLocalizedTextMultiple()
    {
        var localizedParts = new List<string>();

        foreach (var wordID in _wordIDs)
        {
            localizedParts.Add(API.GetText(wordID));
        }

        return string.Join(_separator, localizedParts);
    }

    protected virtual string GetLocalizedTextMultipleEditor()
    {
        var localizedParts = new List<string>();

        foreach (var wordID in _wordIDs)
        {
            localizedParts.Add(wordID.ToString());
        }

        return string.Join(_separator, localizedParts);
    }


    public void AddWordID(WordIDs wordID)
    {
        if (!_wordIDs.Contains(wordID))
        {
            _wordIDs.Add(wordID);
        }
    }

    public void RemoveWordID(WordIDs wordID)
    {
        _wordIDs.Remove(wordID);
    }

    public void ClearWordIDs()
    {
        _wordIDs.Clear();
    }

    public WordIDs GetWordIDs(int index)
    {
        return index < 0 || index >= _wordIDs.Count
            ? throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.")
            : _wordIDs[index];
    }

    public List<WordIDs> GetWordIDs()
    {
        return new List<WordIDs>(_wordIDs);
    }

    public void SetWordIDs(List<WordIDs> newWordIDs)
    {
        _wordIDs = new List<WordIDs>(newWordIDs);
    }

    /// <summary>
    /// Sets the text of the UI element (called in Awake)
    /// </summary>
    public abstract void SetText(string text);

    protected abstract void InitializeComponents();
}
