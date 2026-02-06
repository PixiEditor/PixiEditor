using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.AnimationRenderer.Core;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.DocumentPassthroughActions;
using PixiEditor.Models.Handlers;

namespace PixiEditor.ViewModels.Document;

internal class AnimationDataViewModel : ObservableObject, IAnimationHandler
{
    private int _activeFrameBindable = 1;
    private int frameRateBindable = 60;
    private int onionFrames = 1;
    private double onionOpacity = 50;
    private int defaultEndFrameBindable = 60;
    private bool defaultEndFrameSet = false;

    public DocumentViewModel Document { get; }
    protected DocumentInternalParts Internals { get; }
    public IReadOnlyCollection<ICelHandler> KeyFrames => keyFrames;

    public IReadOnlyCollection<ICelHandler> AllCels => allCels;

    public event Action<int, int> ActiveFrameChanged;

    private KeyFrameCollection keyFrames = new KeyFrameCollection();
    private List<ICelHandler> allCels = new List<ICelHandler>();
    private bool onionSkinningEnabled;
    private bool isPlayingBindable;
    private bool fallbackAnimationToLayerImage;

    private int? cachedFirstFrame;
    private int? cachedLastFrame;

    private bool blockUpdateFrame = false;

    public int ActiveFrameBindable
    {
        get => _activeFrameBindable;
        set
        {
            if (Document.BlockingUpdateableChangeActive || blockUpdateFrame)
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
            if (Document.BlockingUpdateableChangeActive)
                return;

            Internals.ActionAccumulator.AddFinishedActions(new SetFrameRate_Action(value));
        }
    }

    public bool OnionSkinningEnabledBindable
    {
        get => onionSkinningEnabled;
        set
        {
            if (Document.BlockingUpdateableChangeActive)
                return;

            Internals.ActionAccumulator.AddFinishedActions(new ToggleOnionSkinning_PassthroughAction(value));
        }
    }

    public int OnionFramesBindable
    {
        get => onionFrames;
        set
        {
            if (Document.BlockingUpdateableChangeActive)
                return;

            Internals.ActionAccumulator.AddFinishedActions(new SetOnionSettings_Action(value, OnionOpacityBindable));
        }
    }

    public double OnionOpacityBindable
    {
        get => onionOpacity;
        set
        {
            if (Document.BlockingUpdateableChangeActive)
                return;

            Internals.ActionAccumulator.AddFinishedActions(new SetOnionSettings_Action(OnionFramesBindable, value));
        }
    }

    public bool IsPlayingBindable
    {
        get => isPlayingBindable;
        set
        {
            if (Document.BlockingUpdateableChangeActive)
                return;

            Internals.ActionAccumulator.AddFinishedActions(new SetPlayingState_PassthroughAction(value));
        }
    }

    public int DefaultEndFrameBindable
    {
        get => defaultEndFrameBindable;
        set
        {
            if (Document.BlockingUpdateableChangeActive)
                return;

            Internals.ActionAccumulator.AddFinishedActions(new SetDefaultEndFrame_Action(value));
        }
    }

    public bool FallbackAnimationToLayerImageBindable
    {
        get => fallbackAnimationToLayerImage;
        set
        {
            if (Document.BlockingUpdateableChangeActive)
                return;

            Internals.ActionAccumulator.AddFinishedActions(new SetFallbackAnimationToLayerImage_Action(value));
        }
    }

    public int FirstVisibleFrame => cachedFirstFrame ??= keyFrames.Count > 0 ? keyFrames.Min(x => x.StartFrameBindable) : 1;

    public int LastFrame => cachedLastFrame ??= keyFrames.Count > 0
        ? keyFrames.Max(x => x.StartFrameBindable + x.DurationBindable)
        : DefaultEndFrameBindable;

    public int FramesCount => LastFrame - 1;

    private double ActiveNormalizedTime => (double)(ActiveFrameBindable - 1) / (FramesCount - 1);

    public AnimationDataViewModel(DocumentViewModel document, DocumentInternalParts internals)
    {
        Document = document;
        Internals = internals;
        Document.LayersChanged += (sender, args) => SortByLayers();
    }

    internal void InitFrom(IReadOnlyAnimationData documentAnimationData)
    {
        frameRateBindable = documentAnimationData.FrameRate;
        onionFrames = documentAnimationData.OnionFrames;
        onionOpacity = documentAnimationData.OnionOpacity;
        defaultEndFrameBindable = documentAnimationData.DefaultEndFrame;
        foreach (var readOnlyKeyFrame in documentAnimationData.KeyFrames)
        {
            AddKeyFrameInternal(Document, Internals, readOnlyKeyFrame);
        }
    }

    private void AddKeyFrameInternal(DocumentViewModel doc, DocumentInternalParts internals,
        IReadOnlyKeyFrame readOnlyKeyFrame)
    {
        if (readOnlyKeyFrame is IKeyFrameChildrenContainer childrenContainer)
        {
            var groupViewModel = new CelGroupViewModel(readOnlyKeyFrame.StartFrame, readOnlyKeyFrame.Duration,
                readOnlyKeyFrame.NodeId, readOnlyKeyFrame.Id, doc, internals);

            foreach (var child in childrenContainer.Children)
            {
                AddKeyFrameInternal(doc, internals, child);
            }

            AddKeyFrame(groupViewModel);
        }
        else
        {
            var rasterCel = new RasterCelViewModel(readOnlyKeyFrame.NodeId, readOnlyKeyFrame.StartFrame,
                readOnlyKeyFrame.Duration, readOnlyKeyFrame.Id, doc, internals);
            AddKeyFrame(rasterCel);
        }
    }

    public KeyFrameTime ActiveFrameTime => new KeyFrameTime(ActiveFrameBindable, ActiveNormalizedTime);

    public Guid? CreateCel(Guid targetLayerGuid, int frame, Guid? toCloneFrom = null,
        int? frameToCopyFrom = null)
    {
        if (!Document.BlockingUpdateableChangeActive)
        {
            Guid newCelGuid = Guid.NewGuid();
            Internals.ActionAccumulator.AddFinishedActions(new CreateCel_Action(targetLayerGuid,
                newCelGuid, Math.Max(1, frame),
                frameToCopyFrom ?? -1, toCloneFrom ?? Guid.Empty));
            return newCelGuid;
        }

        return null;
    }

    public void DeleteCels(List<Guid> keyFrameIds)
    {
        if (!Document.BlockingUpdateableChangeActive)
        {
            for (var i = 0; i < keyFrameIds.Count; i++)
            {
                var id = keyFrameIds[i];
                if (i == keyFrameIds.Count - 1)
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
        if (!Document.BlockingUpdateableChangeActive)
        {
            Internals.ActionAccumulator.AddActions(new KeyFramesStartPos_Action(infoIds.ToList(), infoDelta));
        }
    }

    public void ToggleOnionSkinning(bool value)
    {
        if (!Document.BlockingUpdateableChangeActive)
        {
            Internals.ActionAccumulator.AddFinishedActions(new ToggleOnionSkinning_PassthroughAction(value));
        }
    }

    public void EndKeyFramesStartPos()
    {
        if (!Document.BlockingUpdateableChangeActive)
        {
            Internals.ActionAccumulator.AddFinishedActions(new EndKeyFramesStartPos_Action());
        }
    }

    public void SetFrameRate(int newFrameRate)
    {
        frameRateBindable = newFrameRate;
        if (!defaultEndFrameSet)
        {
            defaultEndFrameBindable = frameRateBindable;
            defaultEndFrameSet = true;
        }

        OnPropertyChanged(nameof(FrameRateBindable));
        OnPropertyChanged(nameof(DefaultEndFrameBindable));
        OnPropertyChanged(nameof(LastFrame));
        OnPropertyChanged(nameof(FramesCount));
    }

    public void SetDefaultEndFrame(int newDefaultEndFrame)
    {
        if (newDefaultEndFrame < 0)
            return;

        defaultEndFrameBindable = newDefaultEndFrame;
        defaultEndFrameSet = true;
        cachedLastFrame = null;
        OnPropertyChanged(nameof(DefaultEndFrameBindable));
        OnPropertyChanged(nameof(LastFrame));
        OnPropertyChanged(nameof(FramesCount));
    }

    public void SetFallbackAnimationToLayerImage(bool enabled)
    {
        fallbackAnimationToLayerImage = enabled;
        OnPropertyChanged(nameof(FallbackAnimationToLayerImageBindable));
    }

    public void SetActiveFrame(int newFrame)
    {
        int previousFrame = _activeFrameBindable;
        _activeFrameBindable = newFrame;
        ActiveFrameChanged?.Invoke(previousFrame, newFrame);
        blockUpdateFrame = true;
        OnPropertyChanged(nameof(ActiveFrameBindable));
        blockUpdateFrame = false;
    }

    public void SetPlayingState(bool value)
    {
        isPlayingBindable = value;
        OnPropertyChanged(nameof(IsPlayingBindable));
    }

    public void SetOnionSkinning(bool value)
    {
        onionSkinningEnabled = value;
        OnPropertyChanged(nameof(OnionSkinningEnabledBindable));
    }

    public void SetOnionFrames(int frames, double opacity)
    {
        onionFrames = frames;
        onionOpacity = opacity;
        OnPropertyChanged(nameof(OnionFramesBindable));
        OnPropertyChanged(nameof(OnionOpacityBindable));
    }

    public void SetCelLength(Guid keyFrameId, int newStartFrame, int newDuration)
    {
        if (TryFindCels(keyFrameId, out CelViewModel keyFrame))
        {
            cachedFirstFrame = null;
            cachedLastFrame = null;

            keyFrame.SetStartFrame(newStartFrame);
            keyFrame.SetDuration(newDuration);
            keyFrames.NotifyCollectionChanged();

            OnPropertyChanged(nameof(FirstVisibleFrame));
            OnPropertyChanged(nameof(LastFrame));
            OnPropertyChanged(nameof(FramesCount));
        }
    }

    public void SetKeyFrameVisibility(Guid keyFrameId, bool isVisible)
    {
        if (TryFindCels(keyFrameId, out CelViewModel keyFrame))
        {
            keyFrame.SetVisibility(isVisible);
            keyFrames.NotifyCollectionChanged();
        }
    }

    public void AddKeyFrame(ICelHandler iCel)
    {
        Guid id = iCel.LayerGuid;
        if (TryFindCels(id, out CelGroupViewModel foundGroup))
        {
            foundGroup.Children.Add((CelViewModel)iCel);
        }
        else
        {
            var group =
                new CelGroupViewModel(iCel.StartFrameBindable, iCel.DurationBindable, id, id, Document,
                    Internals);
            group.Children.Add((CelViewModel)iCel);
            keyFrames.Add(group);
        }

        keyFrames.NotifyCollectionChanged(NotifyCollectionChangedAction.Add, (CelViewModel)iCel);

        if (!allCels.Contains(iCel))
        {
            allCels.Add(iCel);
        }

        SortByLayers();

        cachedFirstFrame = null;
        cachedLastFrame = null;

        OnPropertyChanged(nameof(FirstVisibleFrame));
        OnPropertyChanged(nameof(LastFrame));
        OnPropertyChanged(nameof(FramesCount));
    }

    public void RemoveKeyFrame(Guid keyFrameId)
    {
        TryFindCels<CelViewModel>(keyFrameId, out _, (frame, parent) =>
        {
            if (frame is not CelGroupViewModel group)
            {
                parent.Children.Remove(frame);
                keyFrames.NotifyCollectionChanged(NotifyCollectionChangedAction.Remove, (CelViewModel)frame);

                if (parent.Children.Count == 0)
                {
                    keyFrames.Remove(parent as CelGroupViewModel);
                }
            }
            else
            {
                keyFrames.Remove(group);
            }
        });

        allCels.RemoveAll(x => x.Id == keyFrameId);

        cachedFirstFrame = null;
        cachedLastFrame = null;

        OnPropertyChanged(nameof(FirstVisibleFrame));
        OnPropertyChanged(nameof(LastFrame));
        OnPropertyChanged(nameof(FramesCount));
    }

    public void AddSelectedKeyFrame(Guid keyFrameId)
    {
        if (TryFindCels(keyFrameId, out CelViewModel keyFrame))
        {
            keyFrame.IsSelected = true;
        }
    }

    public void RemoveSelectedKeyFrame(Guid keyFrameId)
    {
        if (TryFindCels(keyFrameId, out CelViewModel keyFrame))
        {
            keyFrame.IsSelected = false;
        }
    }

    public void ClearSelectedKeyFrames()
    {
        var selectedFrames = keyFrames.SelectChildrenBy<CelViewModel>(x => x.IsSelected);
        foreach (var frame in selectedFrames)
        {
            frame.IsSelected = false;
        }
    }

    public void RemoveKeyFrames(List<Guid> keyFrameIds)
    {
        List<CelViewModel> framesToRemove = new List<CelViewModel>();
        foreach (var keyFrame in keyFrameIds)
        {
            TryFindCels<CelViewModel>(keyFrame, out _, (frame, parent) =>
            {
                parent.Children.Remove(frame);
                framesToRemove.Add((CelViewModel)frame);
            });

            allCels.RemoveAll(x => x.Id == keyFrame);
        }

        keyFrames.NotifyCollectionChanged(NotifyCollectionChangedAction.Remove, framesToRemove);
    }

    public bool FindKeyFrame<T>(Guid guid, out T keyFrameHandler) where T : ICelHandler
    {
        return TryFindCels<T>(keyFrames, null, guid, out keyFrameHandler, null);
    }

    // TODO: Use the same structure functions as layers
    public bool TryFindCels<T>(Guid id, out T? foundKeyFrame,
        Action<ICelHandler, ICelGroupHandler?> onFound = null) where T : ICelHandler
    {
        return TryFindCels(keyFrames, null, id, out foundKeyFrame, onFound);
    }

    private bool TryFindCels<T>(IReadOnlyCollection<ICelHandler> root, ICelGroupHandler parent, Guid id,
        out T? result,
        Action<ICelHandler, ICelGroupHandler?> onFound) where T : ICelHandler
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

            if (frame is ICelGroupHandler { Children.Count: > 0 } group)
            {
                bool found = TryFindCels(group.Children, group, id, out result, onFound);
                if (found)
                {
                    return true;
                }
            }
        }

        result = default;
        return false;
    }

    public void SortByLayers()
    {
        if (keyFrames.Count < 2)
            return;

        var allLayers = Document.StructureHelper.GetAllLayers();

        if (!OrderDifferent(keyFrames, allLayers)) return;

        var unsortedKeyFrames = keyFrames.ToList();
        var layerKeyFrames = new List<CelGroupViewModel>();

        foreach (var layer in allLayers)
        {
            var group = unsortedKeyFrames.FirstOrDefault(x =>
                x is CelGroupViewModel group && group.LayerGuid == layer.Id) as CelGroupViewModel;
            if (group != null)
            {
                layerKeyFrames.Insert(0, group);
            }
        }

        foreach (var remaining in unsortedKeyFrames)
        {
            if (remaining is CelGroupViewModel group && !layerKeyFrames.Contains(group))
            {
                layerKeyFrames.Add(group);
            }
        }

        this.keyFrames = new KeyFrameCollection(layerKeyFrames);
        OnPropertyChanged(nameof(KeyFrames));
    }

    public int GetFirstVisibleFrame()
    {
        return keyFrames.Count > 0 && keyFrames.Any(x => x.IsVisible)
            ? keyFrames.Where(x => x.IsVisible).Min(x => x.StartFrameBindable) : 1;
    }

    public int GetLastVisibleFrame()
    {
        return keyFrames.Count > 0 && keyFrames.Any(x => x.IsVisible)
            ? keyFrames.Where(x => x.IsVisible).Max(x => x.StartFrameBindable + x.DurationBindable)
            : DefaultEndFrameBindable;
    }

    public int GetVisibleFramesCount()
    {
        return GetLastVisibleFrame() - GetFirstVisibleFrame();
    }

    private static bool OrderDifferent(IReadOnlyCollection<ICelHandler> keyFrames,
        IReadOnlyCollection<ILayerHandler> allLayers)
    {
        List<ICelGroupHandler> groups = new List<ICelGroupHandler>();

        foreach (var keyFrame in keyFrames)
        {
            if (keyFrame is ICelGroupHandler group)
            {
                groups.Add(group);
            }
        }

        if (groups.Count != allLayers.Count)
        {
            return true;
        }

        var reversedLayers = allLayers.Reverse().ToList();

        for (int i = 0; i < groups.Count; i++)
        {
            if (groups[0].LayerGuid != reversedLayers.ElementAt(i).Id)
            {
                return true;
            }
        }

        return false;
    }

    public void Dispose()
    {
        foreach (var cel in allCels)
        {
            cel.Dispose();
        }
    }
}
