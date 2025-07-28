namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface ITransparencyLockable
{
    /// <summary>
    /// Locks the transparency of the layer
    /// </summary>
    bool LockTransparency { get; set; }
}
