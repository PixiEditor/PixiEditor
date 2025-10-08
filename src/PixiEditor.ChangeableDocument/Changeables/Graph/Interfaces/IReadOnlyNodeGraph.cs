using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using PixiEditor.Common;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface IReadOnlyNodeGraph : ICacheable, IDisposable
{
    public IReadOnlyBlackboard Blackboard { get; }
    public IReadOnlyCollection<IReadOnlyNode> AllNodes { get; }
    public IReadOnlyNode OutputNode { get; }
    public void AddNode(IReadOnlyNode node);
    public void RemoveNode(IReadOnlyNode node);
    public bool TryTraverse(Action<IReadOnlyNode> action);
    public bool TryTraverse(IReadOnlyNode end, Action<IReadOnlyNode> action);
    public void Execute(RenderContext context);
    public void Execute(IReadOnlyNode end, RenderContext context);
    Queue<IReadOnlyNode> CalculateExecutionQueue(IReadOnlyNode endNode);
    public IReadOnlyNodeGraph Clone();
}
