using System.Collections.ObjectModel;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;

namespace PixiEditor.ViewModels.Document;

internal class GuidesViewModel : ViewModelBase, IGuidesHandler
{
    public IReadOnlyList<IGuideHandler> Guides => guides;

    IEnumerable<IGuideHandler> IGuidesHandler.GuidesHandler => Guides;

    private ObservableCollection<IGuideHandler> guides = new ObservableCollection<IGuideHandler>();
    private ISnappingHandler snappingHandler;

    private DocumentInternalParts internals;

    public GuidesViewModel(ISnappingHandler snappingHandler, DocumentInternalParts documentInternals)
    {
        internals = documentInternals;
        this.snappingHandler = snappingHandler;
    }

    public void AddGuide(bool vertical, double offset)
    {
        if (internals.ChangeController.IsBlockingChangeActive) return;

        internals.ActionAccumulator.AddFinishedActions(new AddGuide_Action(vertical, offset));
    }

    public void RemoveGuide(IGuideHandler guide)
    {
        if (internals.ChangeController.IsBlockingChangeActive) return;

        internals.ActionAccumulator.AddFinishedActions(new RemoveGuide_Action(guide.Id));
    }

    public void AddGuideInternal(IGuideHandler guideViewModel)
    {
        guides.Add(guideViewModel);
        snappingHandler.AddFromAxis("guide_" + guideViewModel.Id, guideViewModel.IsVertical, () => guideViewModel.Offset);
        OnPropertyChanged(nameof(Guides));
    }

    public void RemoveGuideInternal(IGuideHandler guide)
    {
        guides.Remove(guide);
        snappingHandler.Remove("guide_" + guide.Id);
        OnPropertyChanged(nameof(Guides));
    }
}
