namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IReadOnlyKeyFrame
{
    public int StartFrame { get; }
    public int Duration { get; }
    public Guid LayerGuid { get; }
    public Guid Id { get; }
    public bool IsVisible { get; }
    public IReadOnlyLayer TargetLayer { get; }
}
