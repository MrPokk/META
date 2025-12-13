using Gley.Localization;
using UnityEngine;

public abstract class LocalizedUIElement : MonoBehaviour
{
    [SerializeField] protected WordIDs _wordID;

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
        SetText(GetLocalizedText());
    }

    public virtual void UpdateLocalizationEditor()
    {
        SetText(_wordID.ToString());
    }

    protected virtual string GetLocalizedText()
    {
        return API.GetText(_wordID);
    }

    /// <summary>
    /// Sets the text of the UI element (called in Awake)
    /// </summary>
    public abstract void SetText(string text);

    protected abstract void InitializeComponents();
}
