using Avalonia;
using PixiEditor.AvaloniaUI.Helpers.Converters;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.UI.Common.Fonts;

namespace PixiEditor.AvaloniaUI.ViewModels.Dock;

internal class SwatchesDockViewModel : DockableViewModel
{
    public override string Id => "Swatches";
    public override string Title => new LocalizedString("SWATCHES_TITLE");
    public override bool CanFloat => true;
    public override bool CanClose => true;

    private DocumentManagerViewModel documentManagerSubViewModel;

    public DocumentManagerViewModel DocumentManagerSubViewModel
    {
        get => documentManagerSubViewModel;
        set => SetProperty(ref documentManagerSubViewModel, value);
    }

    public SwatchesDockViewModel(DocumentManagerViewModel documentManagerViewModel)
    {
        DocumentManagerSubViewModel = documentManagerViewModel;
        TabCustomizationSettings.Icon = PixiPerfectIcons.ToIconControl(PixiPerfectIcons.Grid);
    }
}
