using System;
using Gley.Localization;

public class UIUpdateLocalized
{
    public static Action OnUpdateLocalized;

    public static void SetLanguage(SupportedLanguages supportedLanguages)
    {
        API.SetCurrentLanguage(supportedLanguages);

        OnUpdateLocalized?.Invoke();
    }
}
