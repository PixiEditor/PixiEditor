namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IReadOnlyRasterKeyFrame : IReadOnlyKeyFrame
{
    public IReadOnlyChunkyImage Image { get; }
}
