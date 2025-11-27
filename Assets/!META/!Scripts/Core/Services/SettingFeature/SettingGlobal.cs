using Gley.Localization;

public class SettingGlobal
{
    private static SettingGlobal _instance;

    public static SettingGlobal Instance => _instance ??= new SettingGlobal();

    private SupportedLanguages _currentLanguage;


    public SettingGlobal()
    {
        _currentLanguage = API.GetCurrentLanguage();
    }
}
