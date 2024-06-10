namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IReadOnlyAnimationData
{
    public int CurrentFrame { get; }
    public IReadOnlyList<IReadOnlyClip> Clips { get; }
}
