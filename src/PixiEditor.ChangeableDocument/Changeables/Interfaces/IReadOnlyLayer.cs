namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IReadOnlyLayer : IReadOnlyStructureMember
{
    /// <summary>
    /// The chunky image of the layer
    /// </summary>
    IReadOnlyChunkyImage LayerImage { get; }
    /// <summary>
    /// Locks the transparency of the layer
    /// </summary>
    bool LockTransparency { get; }
}
