using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class UITextProvider : LocalizedUIElement
{
    private TMP_Text _textComponent;

    protected override void InitializeComponents()
    {
        _textComponent ??= GetComponent<TMP_Text>();
    }

    public override void SetText(string text)
    {
        _textComponent.text = text;
    }
}
