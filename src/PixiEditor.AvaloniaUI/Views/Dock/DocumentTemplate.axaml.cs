using Avalonia.Controls;
using PixiEditor.AvaloniaUI.Models.Preferences;
using PixiEditor.AvaloniaUI.ViewModels.Dock;
using PixiEditor.AvaloniaUI.ViewModels.SubViewModels;
using PixiEditor.AvaloniaUI.ViewModels.Tools.Tools;

namespace PixiEditor.AvaloniaUI.Views.Dock;

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
}

