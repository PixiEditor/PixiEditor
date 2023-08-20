using Avalonia.Controls;
using PixiEditor.AvaloniaUI.Models.Preferences;
using PixiEditor.AvaloniaUI.ViewModels;
using PixiEditor.AvaloniaUI.ViewModels.Tools.Tools;

namespace PixiEditor.AvaloniaUI.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void Viewport_OnContextMenuOpening(object? sender, ContextRequestedEventArgs e)
    {
        ViewModelMain vm = (ViewModelMain)DataContext;
        var tools = vm.ToolsSubViewModel;

        var superSpecialBrightnessTool = tools.RightClickMode == RightClickMode.SecondaryColor && tools.ActiveTool is BrightnessToolViewModel;
        var superSpecialColorPicker = tools.RightClickMode == RightClickMode.Erase && tools.ActiveTool is ColorPickerToolViewModel;

        if (superSpecialBrightnessTool || superSpecialColorPicker)
        {
            e.Handled = true;
            return;
        }

        var useContextMenu = vm.ToolsSubViewModel.RightClickMode == RightClickMode.ContextMenu;
        var usesErase = tools.RightClickMode == RightClickMode.Erase && tools.ActiveTool.IsErasable;
        var usesSecondaryColor = tools.RightClickMode == RightClickMode.SecondaryColor && tools.ActiveTool.UsesColor;

        if (!useContextMenu && (usesErase || usesSecondaryColor))
        {
            e.Handled = true;
        }
    }
}
