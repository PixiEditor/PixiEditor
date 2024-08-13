using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IReadOnlyKeyFrame
{
    public int StartFrame { get; }
    public int Duration { get; }
    public Guid NodeId { get; }
    public Guid Id { get; }
    public bool IsVisible { get; }
}
