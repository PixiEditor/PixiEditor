namespace PixiEditor.Localization;

public class LanguageData
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string LocaleFileName { get; set; }
    
    // https://icons8.com/icon/set/flags/color
    public string IconFileName { get; set; }
    public string IconPath => $"pack://application:,,,/PixiEditor;component/Images/LanguageFlags/{IconFileName}";
    
    public override string ToString()
    {
        return Name;
    }
}
