using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.AvaloniaUI.Models.DocumentModels;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.AvaloniaUI.ViewModels.Document;

internal class AnimationDataViewModel : ObservableObject, IAnimationHandler
{
    private int _activeFrameBindable;
    public DocumentViewModel Document { get; }
    protected DocumentInternalParts Internals { get; }
    public ObservableCollection<IKeyFrameHandler> KeyFrames { get; } = new();

    public int ActiveFrameBindable
    {
        get => _activeFrameBindable;
        set
        {
            if (Document.UpdateableChangeActive)
                return;
            
            Internals.ActionAccumulator.AddFinishedActions(
                new ActiveFrame_Action(value),
                new EndActiveFrame_Action());
        }
    }

    public AnimationDataViewModel(DocumentViewModel document, DocumentInternalParts internals)
    {
        Document = document;
        Internals = internals;
    }

    public void AddRasterClip(Guid targetLayerGuid, int frame, bool cloneFromExisting)
    {
        if (!Document.UpdateableChangeActive)
            Internals.ActionAccumulator.AddFinishedActions(new CreateRasterClip_Action(targetLayerGuid, frame, cloneFromExisting));
    }

    public void SetActiveFrame(int newFrame)
    {
        _activeFrameBindable = newFrame;
        OnPropertyChanged(nameof(ActiveFrameBindable));
    }
}
