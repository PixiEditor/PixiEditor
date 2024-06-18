using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    public IReadOnlyCollection<IKeyFrameHandler> KeyFrames => keyFrames;

    private KeyFrameCollection keyFrames = new KeyFrameCollection();

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

    public void CreateRasterKeyFrame(Guid targetLayerGuid, int frame, bool cloneFromExisting)
    {
        if (!Document.UpdateableChangeActive)
        {
            Internals.ActionAccumulator.AddFinishedActions(new CreateRasterKeyFrame_Action(targetLayerGuid, frame,
                cloneFromExisting));
        }
    }

    public void DeleteKeyFrame(Guid keyFrameId)
    {
        if (!Document.UpdateableChangeActive)
        {
            Internals.ActionAccumulator.AddFinishedActions(new DeleteKeyFrame_Action(keyFrameId));
        }
    }
    
    public void SetActiveFrame(int newFrame)
    {
        _activeFrameBindable = newFrame;
        OnPropertyChanged(nameof(ActiveFrameBindable));
    }

    public void SetFrameLength(Guid keyFrameId, int newStartFrame, int newDuration)
    {
        if(TryFindKeyFrame(keyFrameId, out KeyFrameViewModel keyFrame))
        {
            keyFrame.SetStartFrame(newStartFrame);
            keyFrame.SetDuration(newDuration);
            keyFrames.NotifyCollectionChanged();
        }
    }

    public void AddKeyFrame(IKeyFrameHandler keyFrame)
    {
        Guid id = keyFrame.LayerGuid;
        if (TryFindKeyFrame(id, out KeyFrameGroupViewModel foundGroup))
        {
            foundGroup.Children.Add((KeyFrameViewModel)keyFrame);
        }
        else
        {
            var group =
                new KeyFrameGroupViewModel(keyFrame.StartFrameBindable, keyFrame.DurationBindable, id, id, Document, Internals);
            group.Children.Add((KeyFrameViewModel)keyFrame);
            keyFrames.Add(group);
        }

        keyFrames.NotifyCollectionChanged(NotifyCollectionChangedAction.Add, (KeyFrameViewModel)keyFrame);
    }

    public void RemoveKeyFrame(Guid keyFrameId)
    {
        TryFindKeyFrame<KeyFrameViewModel>(keyFrameId, out _, (frame, parent) =>
        {
            parent.Children.Remove(frame);
            keyFrames.NotifyCollectionChanged(NotifyCollectionChangedAction.Remove, (KeyFrameViewModel)frame);
        });
        
    }
    
    public bool FindKeyFrame<T>(Guid guid, out T keyFrameHandler) where T : IKeyFrameHandler
    {
        return TryFindKeyFrame<T>(keyFrames, null, guid, out keyFrameHandler, null);
    }

    // TODO: Use the same structure functions as layers
    public bool TryFindKeyFrame<T>(Guid id, out T? foundKeyFrame,
        Action<IKeyFrameHandler, IKeyFrameGroupHandler?> onFound = null) where T : IKeyFrameHandler
    {
        return TryFindKeyFrame(keyFrames, null, id, out foundKeyFrame, onFound);
    }

    private bool TryFindKeyFrame<T>(IReadOnlyCollection<IKeyFrameHandler> root, IKeyFrameGroupHandler parent, Guid id, out T? result,
        Action<IKeyFrameHandler, IKeyFrameGroupHandler?> onFound) where T : IKeyFrameHandler
    {
        for (var i = 0; i < root.Count; i++)
        {
            var frame = root.ElementAt(i);
            if (frame is T targetFrame && targetFrame.Id.Equals(id))
            {
                result = targetFrame;
                onFound?.Invoke(frame, parent);
                return true;
            }

            if (frame is IKeyFrameGroupHandler { Children.Count: > 0 } group)
            {
                bool found = TryFindKeyFrame(group.Children, group, id, out result, onFound);
                if (found)
                {
                    return true;
                }
            }
        }

        result = default;
        return false;
    }
}
