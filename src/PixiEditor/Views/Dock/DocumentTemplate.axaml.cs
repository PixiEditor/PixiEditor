using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PixiEditor.Models.Preferences;
using PixiEditor.ViewModels.Dock;
using PixiEditor.Views.Palettes;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.ViewModels.SubViewModels;
using PixiEditor.ViewModels.Tools.Tools;

namespace PixiEditor.Views.Dock;

public partial class DocumentTemplate : UserControl
{
    public DocumentTemplate()
    {
        InitializeComponent();
    }

    private void Viewport_OnContextMenuOpening(object? sender, ContextRequestedEventArgs e)
    {
        ViewportWindowViewModel vm = ((ViewportWindowViewModel)DataContext);
        var tools = vm.Owner.Owner.ToolsSubViewModel;

        var superSpecialBrightnessTool = tools.RightClickMode == RightClickMode.SecondaryColor && tools.ActiveTool is BrightnessToolViewModel;
        var superSpecialColorPicker = tools.RightClickMode == RightClickMode.Erase && tools.ActiveTool is ColorPickerToolViewModel;

        if (superSpecialBrightnessTool || superSpecialColorPicker)
        {
            e.Handled = true;
            return;
        }

        var useContextMenu = vm.Owner.Owner.ToolsSubViewModel.RightClickMode == RightClickMode.ContextMenu;
        var usesErase = tools.RightClickMode == RightClickMode.Erase && tools.ActiveTool.IsErasable;
        var usesSecondaryColor = tools.RightClickMode == RightClickMode.SecondaryColor && tools.ActiveTool.UsesColor;

        if (!useContextMenu && (usesErase || usesSecondaryColor))
        {
            e.Handled = true;
        }
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        Viewport?.ContextFlyout?.Hide();
    }

    private void MenuItem_OnClick(object? sender, PointerReleasedEventArgs e)
    {
        Viewport?.ContextFlyout?.Hide();
    }
}

