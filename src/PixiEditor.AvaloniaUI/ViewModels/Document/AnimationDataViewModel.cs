using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.AvaloniaUI.Models.DocumentModels;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.AvaloniaUI.ViewModels.Document;

internal class AnimationDataViewModel : ObservableObject, IAnimationHandler
{
    public DocumentViewModel Document { get; }
    protected DocumentInternalParts Internals { get; }
    public ObservableCollection<IClipHandler> Clips { get; } = new();

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
}
