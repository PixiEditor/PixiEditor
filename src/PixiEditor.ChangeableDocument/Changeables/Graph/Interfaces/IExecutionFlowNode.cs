namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface IExecutionFlowNode
{
    public HashSet<IReadOnlyNode> HandledNodes { get; }
}
