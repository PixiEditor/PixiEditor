using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface IReadOnlyNodeGraph
{
    public IReadOnlyCollection<IReadOnlyNode> AllNodes { get; }
    public IReadOnlyNode OutputNode { get; }
    public void AddNode(IReadOnlyNode node);
    public void RemoveNode(IReadOnlyNode node);
    public bool TryTraverse(Action<IReadOnlyNode> action);
    public void Execute(RenderContext context);
    Queue<IReadOnlyNode> CalculateExecutionQueue(IReadOnlyNode endNode);
}
