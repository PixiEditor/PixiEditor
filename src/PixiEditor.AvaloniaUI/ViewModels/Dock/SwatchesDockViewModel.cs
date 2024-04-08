using Avalonia;
using PixiEditor.AvaloniaUI.Helpers.Converters;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.AvaloniaUI.ViewModels.Dock;

internal class SwatchesDockViewModel : DockableViewModel
{
    public override string Id => "Swatches";
    public override string Title => new LocalizedString("SWATCHES_DOCKABLE_TITLE");
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
        TabCustomizationSettings.Icon = ImagePathToBitmapConverter.TryLoadBitmapFromRelativePath("/Images/Dockables/Swatches.png");
    }
}
