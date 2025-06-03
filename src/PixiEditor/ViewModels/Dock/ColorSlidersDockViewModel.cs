using Avalonia;
using Avalonia.Media;
using PixiEditor.Helpers.Converters;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.SubViewModels;

namespace PixiEditor.ViewModels.Dock;

internal class ColorSlidersDockViewModel : DockableViewModel
{
    public const string TabId = "ColorSliders";
    public override string Id => TabId;
    public override string Title => new LocalizedString("COLOR_SLIDERS_TITLE");
    public override bool CanFloat => true;
    public override bool CanClose => true;

    private ColorsViewModel colorsSubViewModel;

    public ColorsViewModel ColorsSubViewModel
    {
        get => colorsSubViewModel;
        set => SetProperty(ref colorsSubViewModel, value);
    }

    public ColorSlidersDockViewModel(ColorsViewModel colorsSubVm)
    {
        ColorsSubViewModel = colorsSubVm;
        TabCustomizationSettings.Icon = PixiPerfectIconExtensions.ToIcon(PixiPerfectIcons.ColorSliders);
    }
}
