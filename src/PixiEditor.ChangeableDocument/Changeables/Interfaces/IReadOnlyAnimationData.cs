namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IReadOnlyAnimationData
{
    public int ActiveFrame { get; }
    public IReadOnlyList<IReadOnlyKeyFrame> KeyFrames { get; }
}
