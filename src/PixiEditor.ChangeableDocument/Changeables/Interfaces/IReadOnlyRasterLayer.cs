namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IReadOnlyRasterLayer : ITransparencyLockable, IChunkyImageProperty
{
    /// <summary>
    /// The chunky image of the layer
    /// </summary>
    IReadOnlyChunkyImage LayerImage { get; }
}
