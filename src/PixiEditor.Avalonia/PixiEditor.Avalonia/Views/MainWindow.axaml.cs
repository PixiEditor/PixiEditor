using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using PixiEditor.Models.Localization;
using Splat;

namespace PixiEditor.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        LocalizationProvider localizationProvider = new LocalizationProvider(null);
        localizationProvider.LoadData(/*TODO: IPreferences.Current.GetPreference<string>("LanguageCode");*/);

        InitializeComponent();
    }
}
