namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IReadOnlyAnimationData
{
    public int FrameRate { get; }
    public IReadOnlyList<IReadOnlyKeyFrame> KeyFrames { get; }
    public int OnionFrames { get; }
    public double OnionOpacity { get; }
    public int DefaultEndFrame { get; }
    public bool FallbackAnimationToLayerImage { get; }
    public bool TryFindKeyFrame<T>(Guid id, out T keyFrame) where T : IReadOnlyKeyFrame;
    public IReadOnlyAnimationData Clone();
}
