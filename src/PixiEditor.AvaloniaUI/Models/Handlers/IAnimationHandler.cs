namespace PixiEditor.AvaloniaUI.Models.Handlers;

public interface IAnimationHandler
{
    public IReadOnlyCollection<IKeyFrameHandler> KeyFrames { get; }
    public int ActiveFrameBindable { get; set; }
    public void CreateRasterKeyFrame(Guid targetLayerGuid, int frame, bool cloneFromExisting);
    public void SetActiveFrame(int newFrame);
    public bool FindKeyFrame<T>(Guid guid, out T keyFrameHandler) where T : IKeyFrameHandler;
    internal void AddKeyFrame(IKeyFrameHandler keyFrame);
    internal void RemoveKeyFrame(Guid keyFrameId);
}
