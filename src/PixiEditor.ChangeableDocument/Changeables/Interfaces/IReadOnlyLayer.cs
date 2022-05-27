namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IReadOnlyLayer : IReadOnlyStructureMember
{
    IReadOnlyChunkyImage LayerImage { get; }
    bool LockTransparency { get; }
}
