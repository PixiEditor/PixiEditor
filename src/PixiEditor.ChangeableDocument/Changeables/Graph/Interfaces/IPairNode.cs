using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface IPairNode
{
    public Guid OtherNode { get; set; }
}
