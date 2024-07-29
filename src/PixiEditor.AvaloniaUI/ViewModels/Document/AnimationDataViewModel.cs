using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.AnimationRenderer.Core;
using PixiEditor.AvaloniaUI.Models.DocumentModels;
using PixiEditor.AvaloniaUI.Models.DocumentPassthroughActions;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.AvaloniaUI.ViewModels.Document;

internal class AnimationDataViewModel : ObservableObject, IAnimationHandler
{
    private int _activeFrameBindable = 1;
    private int frameRateBindable = 60;
    public DocumentViewModel Document { get; }
    protected DocumentInternalParts Internals { get; }
    public IReadOnlyCollection<IKeyFrameHandler> KeyFrames => keyFrames;
    
    public IReadOnlyCollection<IKeyFrameHandler> AllKeyFrames => allKeyFrames;

    private KeyFrameCollection keyFrames = new KeyFrameCollection();
    private List<IKeyFrameHandler> allKeyFrames = new List<IKeyFrameHandler>();

    public int ActiveFrameBindable
    {
        get => _activeFrameBindable;
        set
        {
            if (Document.UpdateableChangeActive)
                return;

            Internals.ActionAccumulator.AddActions(new SetActiveFrame_PassthroughAction(value));
        }
    }

    public IAnimationRenderer Renderer { get; set; }

    public int FrameRateBindable
    {
        get => frameRateBindable;
        set
        {
            if (Document.UpdateableChangeActive)
                return;

            Internals.ActionAccumulator.AddFinishedActions(new SetFrameRate_Action(value)); 
        }
    }

    public int FirstFrame => keyFrames.Count > 0 ? keyFrames.Min(x => x.StartFrameBindable) : 0;
    public int LastFrame => keyFrames.Count > 0 ? keyFrames.Max(x => x.StartFrameBindable + x.DurationBindable) 
        : DefaultEndFrame;
    public int FramesCount => LastFrame - FirstFrame + 1;
    
    private double ActiveNormalizedTime => (double)(ActiveFrameBindable - FirstFrame) / FramesCount;

    private int DefaultEndFrame => FrameRateBindable; // 1 second

    public AnimationDataViewModel(DocumentViewModel document, DocumentInternalParts internals)
    {
        Document = document;
        Internals = internals;
    }

    public KeyFrameTime ActiveFrameTime => new KeyFrameTime(ActiveFrameBindable, ActiveNormalizedTime);

    public void CreateRasterKeyFrame(Guid targetLayerGuid, int frame, Guid? toCloneFrom = null, int? frameToCopyFrom = null)
    {
        if (!Document.UpdateableChangeActive)
        {
            Internals.ActionAccumulator.AddFinishedActions(new CreateRasterKeyFrame_Action(targetLayerGuid, Guid.NewGuid(), Math.Max(1, frame),
                frameToCopyFrom ?? -1, toCloneFrom ?? Guid.Empty));
        }
    }

    public void DeleteKeyFrames(List<Guid> keyFrameIds)
    {
        if (!Document.UpdateableChangeActive)
        {
            for (var i = 0; i < keyFrameIds.Count; i++)
            {
                var id = keyFrameIds[i];
                if(i == keyFrameIds.Count - 1)
                {
                    Internals.ActionAccumulator.AddFinishedActions(new DeleteKeyFrame_Action(id));
                }
                else
                {
                    Internals.ActionAccumulator.AddActions(new DeleteKeyFrame_Action(id));
                }
            }
        }
    }
    
    public void ChangeKeyFramesStartPos(Guid[] infoIds, int infoDelta)
    {
        if (!Document.UpdateableChangeActive)
        {
            Internals.ActionAccumulator.AddActions(new KeyFramesStartPos_Action(infoIds.ToList(), infoDelta));
        }
    }
    
    public void EndKeyFramesStartPos()
    {
        if (!Document.UpdateableChangeActive)
        {
            Internals.ActionAccumulator.AddFinishedActions(new EndKeyFramesStartPos_Action());
        }
    }
    
    public void SetFrameRate(int newFrameRate)
    {
        frameRateBindable = newFrameRate;
        OnPropertyChanged(nameof(FrameRateBindable));
        OnPropertyChanged(nameof(DefaultEndFrame));
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
    
    public void SetKeyFrameVisibility(Guid keyFrameId, bool isVisible)
    {
        if(TryFindKeyFrame(keyFrameId, out KeyFrameViewModel keyFrame))
        {
            keyFrame.SetVisibility(isVisible);
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

        if (!allKeyFrames.Contains(keyFrame))
        {
            allKeyFrames.Add(keyFrame);
        }
    }

    public void RemoveKeyFrame(Guid keyFrameId)
    {
        TryFindKeyFrame<KeyFrameViewModel>(keyFrameId, out _, (frame, parent) =>
        {
            parent.Children.Remove(frame);
            keyFrames.NotifyCollectionChanged(NotifyCollectionChangedAction.Remove, (KeyFrameViewModel)frame);
        });
        
        allKeyFrames.RemoveAll(x => x.Id == keyFrameId);
    }

    public void AddSelectedKeyFrame(Guid keyFrameId)
    {
        if (TryFindKeyFrame(keyFrameId, out KeyFrameViewModel keyFrame))
        {
            keyFrame.IsSelected = true;
        }
    }

    public void RemoveSelectedKeyFrame(Guid keyFrameId)
    {
        if (TryFindKeyFrame(keyFrameId, out KeyFrameViewModel keyFrame))
        {
            keyFrame.IsSelected = false;
        }
    }

    public void ClearSelectedKeyFrames()
    {
        var selectedFrames = keyFrames.SelectChildrenBy<KeyFrameViewModel>(x => x.IsSelected);
        foreach (var frame in selectedFrames)
        {
            frame.IsSelected = false;
        }
    }

    public void RemoveKeyFrames(List<Guid> keyFrameIds)
    {
        List<KeyFrameViewModel> framesToRemove = new List<KeyFrameViewModel>();
        foreach (var keyFrame in keyFrameIds)
        {
            TryFindKeyFrame<KeyFrameViewModel>(keyFrame, out _, (frame, parent) =>
            {
                parent.Children.Remove(frame);
                framesToRemove.Add((KeyFrameViewModel)frame);
            });
            
            allKeyFrames.RemoveAll(x => x.Id == keyFrame);
        }
        
        keyFrames.NotifyCollectionChanged(NotifyCollectionChangedAction.Remove, framesToRemove);
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
