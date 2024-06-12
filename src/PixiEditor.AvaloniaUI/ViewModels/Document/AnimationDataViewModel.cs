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
            Internals.ActionAccumulator.AddFinishedActions(new CreateRasterClip_Action(targetLayerGuid, frame,
                cloneFromExisting));
    }

    public void SetActiveFrame(int newFrame)
    {
        _activeFrameBindable = newFrame;
        OnPropertyChanged(nameof(ActiveFrameBindable));
    }

    public void AddKeyFrame(IKeyFrameHandler keyFrame)
    {
        Guid id = keyFrame.LayerGuid;
        if (TryFindKeyFrame(id, out KeyFrameGroupViewModel group))
        {
            group.Children.Add((KeyFrameViewModel)keyFrame);
        }
        else
        {
            KeyFrameGroupViewModel createdGroup =
                new KeyFrameGroupViewModel(keyFrame.StartFrame, keyFrame.Duration, id, id);
            createdGroup.Children.Add((KeyFrameViewModel)keyFrame);
            keyFrames.Add(createdGroup);
        }

        keyFrames.NotifyCollectionChanged();
    }

    public void RemoveKeyFrame(Guid keyFrameId)
    {
        TryFindKeyFrame<KeyFrameViewModel>(keyFrameId, out _, (frame, parent) =>
        {
            parent.Children.Remove(frame);
        });
    }

    // TODO: Use the same structure functions as layers
    public bool TryFindKeyFrame<T>(Guid id, out T foundKeyFrame,
        Action<KeyFrameViewModel, KeyFrameGroupViewModel?> onFound = null) where T : KeyFrameViewModel
    {
        return TryFindKeyFrame(keyFrames, null, id, out foundKeyFrame, onFound);
    }

    private bool TryFindKeyFrame<T>(IList<KeyFrameViewModel> root, KeyFrameGroupViewModel parent, Guid id, out T result,
        Action<KeyFrameViewModel, KeyFrameGroupViewModel?> onFound) where T : KeyFrameViewModel
    {
        for (var i = 0; i < root.Count; i++)
        {
            var frame = root[i];
            if (frame is T targetFrame && targetFrame.Id.Equals(id))
            {
                result = targetFrame;
                onFound?.Invoke(frame, parent);
                return true;
            }

            if (frame is KeyFrameGroupViewModel { Children.Count: > 0 } group)
            {
                bool found = TryFindKeyFrame(group.Children, group, id, out result, onFound);
                if (found)
                {
                    return true;
                }
            }
        }

        result = null;
        return false;
    }
}
