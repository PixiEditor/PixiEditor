using Dock.Model.Avalonia.Controls;
using PixiEditor.AvaloniaUI.ViewModels.SubViewModels;

namespace PixiEditor.AvaloniaUI.ViewModels.Dock;

internal class ColorPickerDockViewModel : Tool
{
    private ColorsViewModel colorsViewModel;
    public ColorPickerDockViewModel(ColorsViewModel colorsVm)
    {
        colorsViewModel = colorsVm;
    }
}
