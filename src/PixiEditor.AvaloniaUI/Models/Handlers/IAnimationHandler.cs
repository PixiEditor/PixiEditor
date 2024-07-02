namespace PixiEditor.AvaloniaUI.Models.Handlers;

internal interface IAnimationHandler
{
    public IReadOnlyCollection<IKeyFrameHandler> KeyFrames { get; }
    public int ActiveFrameBindable { get; set; }
    public void CreateRasterKeyFrame(Guid targetLayerGuid, int frame, Guid? toCloneFrom = null, int? frameToCopyFrom = null);
    public void SetActiveFrame(int newFrame);
    public void SetFrameLength(Guid keyFrameId, int newStartFrame, int newDuration);
    public void SetKeyFrameVisibility(Guid infoKeyFrameId, bool infoIsVisible);
    public bool FindKeyFrame<T>(Guid guid, out T keyFrameHandler) where T : IKeyFrameHandler;
    internal void AddKeyFrame(IKeyFrameHandler keyFrame);
    internal void RemoveKeyFrame(Guid keyFrameId);
    public void AddSelectedKeyFrame(Guid keyFrameId);
    public void RemoveSelectedKeyFrame(Guid keyFrameId);
    public void ClearSelectedKeyFrames();
}
