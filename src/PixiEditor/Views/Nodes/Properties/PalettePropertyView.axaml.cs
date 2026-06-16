using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;
using PixiEditor.ViewModels;
using PixiEditor.ViewModels.Nodes.Properties;

namespace PixiEditor.Views.Nodes.Properties;

public partial class PalettePropertyView : NodePropertyView
{
    public PalettePropertyView()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        e.Handled = true;
    }

    private async void ImportFromFile_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not PalettePropertyViewModel vm)
            return;

        await Application.Current.ForDesktopMainWindowAsync(async window =>
        {
            var provider = ViewModelMain.Current.ColorsSubViewModel.PaletteProvider;
            var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                FileTypeFilter = PaletteHelpers.GetFilter(provider.AvailableParsers, true),
            });

            if (files is null || files.Count == 0)
                return;

            var data = await PaletteHelpers.GetValidParser(provider.AvailableParsers, files[0].Path.LocalPath);
            if (data is null)
                return;

            vm.ImportColors(data.Colors.Select(c => c.ToColor()));
        });
    }
}
