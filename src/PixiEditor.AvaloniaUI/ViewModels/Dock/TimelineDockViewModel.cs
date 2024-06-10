using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.AvaloniaUI.ViewModels.Dock;

internal class TimelineDockViewModel : DockableViewModel
{
    public const string TabId = "Timeline";

    public override string Id => TabId;
    public override string Title => new LocalizedString("TIMELINE_TITLE");
    public override bool CanFloat => true;
    public override bool CanClose => true;
    
    private DocumentManagerViewModel documentManagerSubViewModel;

    public DocumentManagerViewModel DocumentManagerSubViewModel
    {
        get => documentManagerSubViewModel;
        set => SetProperty(ref documentManagerSubViewModel, value);
    }

    public TimelineDockViewModel(DocumentManagerViewModel documentManagerViewModel)
    {
        DocumentManagerSubViewModel = documentManagerViewModel;
    }
}
