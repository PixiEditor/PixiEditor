using Avalonia;
using Avalonia.Media;
using PixiEditor.AvaloniaUI.Helpers.Converters;
using PixiEditor.AvaloniaUI.ViewModels.SubViewModels;
using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.AvaloniaUI.ViewModels.Dock;

internal class ColorPickerDockViewModel : DockableViewModel
{
    public const string TabId = "ColorPicker";
    public override string Id => TabId;
    public override string Title => new LocalizedString("COLOR_PICKER_DOCKABLE_TITLE");
    public override bool CanFloat => true;
    public override bool CanClose => true;

    private ColorsViewModel colorsSubViewModel;

    public ColorsViewModel ColorsSubViewModel
    {
        get => colorsSubViewModel;
        set => SetProperty(ref colorsSubViewModel, value);
    }

    public ColorPickerDockViewModel(ColorsViewModel colorsSubVm)
    {
        ColorsSubViewModel = colorsSubVm;
        TabCustomizationSettings.Icon =
            ImagePathToBitmapConverter.TryLoadBitmapFromRelativePath("/Images/Dockables/ColorPicker.png");
    }
}
