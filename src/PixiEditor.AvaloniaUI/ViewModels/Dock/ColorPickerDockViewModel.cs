using Avalonia;
using Dock.Model.Avalonia.Controls;
using PixiEditor.AvaloniaUI.ViewModels.SubViewModels;

namespace PixiEditor.AvaloniaUI.ViewModels.Dock;

internal class ColorPickerDockViewModel : Tool
{
    public static readonly StyledProperty<ColorsViewModel> ColorsViewModelProperty = AvaloniaProperty.Register<ColorPickerDockViewModel, ColorsViewModel>(
        nameof(ColorsSubViewModel));

    public ColorsViewModel ColorsSubViewModel
    {
        get => GetValue(ColorsViewModelProperty);
        set => SetValue(ColorsViewModelProperty, value);
    }

    public ColorPickerDockViewModel(ColorsViewModel colorsSubVm)
    {
        ColorsSubViewModel = colorsSubVm;
    }
}
