using Avalonia;
using PixiEditor.Helpers.Converters;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.ViewModels.Dock;

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
        TabCustomizationSettings.Icon = UI.Common.Fonts.PixiPerfectIconExtensions.ToIcon(PixiPerfectIcons.Swatches);
    }
}
