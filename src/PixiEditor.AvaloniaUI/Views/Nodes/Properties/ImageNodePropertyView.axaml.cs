using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using ChunkyImageLib;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Models.IO;
using PixiEditor.Numerics;

namespace PixiEditor.AvaloniaUI.Views.Nodes.Properties;

public partial class ImageNodePropertyView : NodePropertyView<Surface> 
{
    public ImageNodePropertyView()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var filter = SupportedFilesHelper.BuildOpenFilter();

        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var dialog = desktop.MainWindow.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions { FileTypeFilter = filter });

            var result = dialog.Result;

            if (result.Count == 0 || !Importer.IsSupportedFile(result[0].Path.LocalPath))
                return;

            SetValue(Importer.ImportImage(result[0].Path.LocalPath, VecI.NegativeOne));
        }
    }
}
