using Avalonia.Media;
using PixiEditor.AvaloniaUI.Helpers.Converters;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.AvaloniaUI.ViewModels.SubViewModels;
using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.AvaloniaUI.ViewModels.Dock;

internal class NavigationDockViewModel : DockableViewModel
{
    public const string TabId = "Navigator";

    public override string Id => TabId;
    public override string Title => new LocalizedString("NAVIGATION_TITLE");
    public override bool CanFloat => true;
    public override bool CanClose => true;

    private ColorsViewModel colorsSubViewModel;

    public ColorsViewModel ColorsSubViewModel
    {
        get => colorsSubViewModel;
        set => SetProperty(ref colorsSubViewModel, value);
    }

    private DocumentManagerViewModel documentManagerSubViewModel;

    public DocumentManagerViewModel DocumentManagerSubViewModel
    {
        get => documentManagerSubViewModel;
        set => SetProperty(ref documentManagerSubViewModel, value);
    }

    public NavigationDockViewModel(ColorsViewModel colorsSubViewModel, DocumentManagerViewModel documentManagerViewModel)
    {
        ColorsSubViewModel = colorsSubViewModel;
        DocumentManagerSubViewModel = documentManagerViewModel;
        TabCustomizationSettings.Icon = ImagePathToBitmapConverter.TryLoadBitmapFromRelativePath("/Images/Dockables/Navigator.png");
    }
}
