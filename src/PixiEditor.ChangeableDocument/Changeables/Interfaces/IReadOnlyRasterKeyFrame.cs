namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IReadOnlyRasterKeyFrame : IReadOnlyKeyFrame
{
    IReadOnlyChunkyImage Image { get; }
}
