namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IReadOnlyRasterLayer : ITransparencyLockable
{
    /// <summary>
    /// The chunky image of the layer
    /// </summary>
    IReadOnlyChunkyImage GetLayerImageAtFrame(int frame);
    void SetLayerImageAtFrame(int frame, IReadOnlyChunkyImage image);
}
