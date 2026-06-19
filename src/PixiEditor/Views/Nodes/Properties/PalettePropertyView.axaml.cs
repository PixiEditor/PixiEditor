using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Helpers.Extensions;
using PixiEditor.ViewModels;
using PixiEditor.ViewModels.Nodes.Properties;
using PixiEditor.Views.Windows;

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

    private async void BrowsePalettes_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not PalettePropertyViewModel vm)
            return;

        var browser = PalettesBrowser.Open();
        browser.UsePaletteTooltipKey = "USE_PALETTE";
        browser.ImportPaletteCommand = new RelayCommand<List<PaletteColor>>(colors =>
        {
            if (colors is null)
                return;

            vm.ImportColors(colors.Select(c => c.ToColor()));
        });

        await browser.UpdatePaletteList();
    }

    private void ImportFromDock_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not PalettePropertyViewModel vm)
            return;

        var palette = ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument?.Palette;
        if (palette is null || palette.Count == 0)
            return;

        vm.ImportColors(palette.Select(c => c.ToColor()));
    }
}
