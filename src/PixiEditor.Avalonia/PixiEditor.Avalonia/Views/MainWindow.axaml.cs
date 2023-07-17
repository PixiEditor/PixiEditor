using System.Collections.Generic;
using Avalonia.Controls;
using PixiEditor.Models.AppExtensions;
using PixiEditor.Models.Localization;

namespace PixiEditor.Avalonia.Views;

internal partial class MainWindow : Window
{
    public MainWindow(ExtensionLoader extensionLoader)
    {
        LocalizationProvider localizationProvider = new LocalizationProvider(null);
        localizationProvider.LoadData(/*TODO: IPreferences.Current.GetPreference<string>("LanguageCode");*/);

        InitializeComponent();
    }

    public static MainWindow CreateWithDocuments(IEnumerable<(string? originalPath, byte[] dotPixiBytes)> documents)
    {
        //TODO: Implement this
        /*MainWindow window = new(extLoader);
        FileViewModel fileVM = window.services.GetRequiredService<FileViewModel>();

        foreach (var (path, bytes) in documents)
        {
            fileVM.OpenRecoveredDotPixi(path, bytes);
        }

        return window;*/

        return new MainWindow(null);
    }
}
