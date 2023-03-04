namespace PixiEditor.Localization;

public interface ILocalizationProvider
{
    public static ILocalizationProvider Current => ViewModelMain.Current.LocalizationProvider;
}
