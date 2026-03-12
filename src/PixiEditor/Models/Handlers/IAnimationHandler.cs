using PixiEditor.ChangeableDocument.Changeables.Animations;

namespace PixiEditor.Models.Handlers;

internal interface IAnimationHandler : IDisposable
{
    public IReadOnlyCollection<ICelHandler> KeyFrames { get; }
    public int ActiveFrameBindable { get; set; }
    public KeyFrameTime ActiveFrameTime { get; }
    public bool OnionSkinningEnabledBindable { get; set; }
    public int OnionFramesBindable { get; set; }
    public double OnionOpacityBindable { get; set; }
    public bool IsPlayingBindable { get; set; }
    public Guid? CreateCel(Guid targetLayerGuid, int frame, Guid? toCloneFrom = null, int? frameToCopyFrom = null);
    public void SetFrameRate(int newFrameRate);
    public void SetActiveFrame(int newFrame);
    public void SetCelLength(Guid keyFrameId, int newStartFrame, int newDuration);
    public void SetKeyFrameVisibility(Guid infoKeyFrameId, bool infoIsVisible);
    public bool FindKeyFrame<T>(Guid guid, out T keyFrameHandler) where T : ICelHandler;
    internal void AddKeyFrame(ICelHandler iCel);
    internal void RemoveKeyFrame(Guid keyFrameId);
    public void AddSelectedKeyFrame(Guid keyFrameId);
    public void RemoveSelectedKeyFrame(Guid keyFrameId);
    public void ClearSelectedKeyFrames();
    public void SetOnionSkinning(bool enabled);
    public int FirstVisibleFrame { get; }
    public int LastFrame { get; }
    public void SetOnionFrames(int frames, double opacity);
    public void SetPlayingState(bool play);
    public void SetDefaultEndFrame(int newEndFrame);
    public void SetFallbackAnimationToLayerImage(bool enabled);
}
