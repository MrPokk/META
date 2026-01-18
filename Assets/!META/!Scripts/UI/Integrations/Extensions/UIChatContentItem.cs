using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class UIChatContentItem : MonoBehaviour
{
    private TMP_Text _textComponent;
    private void Awake()
    {
        _textComponent = GetComponent<TMP_Text>();
    }

    public void SetText(string text)
    {
        _textComponent.text = text;
    }
}
