using System.Collections.Immutable;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public class RenderNodeGraph : IReadOnlyNodeGraph, IDisposable
{
    private ImmutableList<IReadOnlyNode>? cachedExecutionList;
    
    private readonly List<Node> _nodes = new();
    public IReadOnlyCollection<Node> Nodes => _nodes;
    public Node? OutputNode => CustomOutputNode ?? Nodes.OfType<OutputNode>().FirstOrDefault();
    public Node? CustomOutputNode { get; set; }

    IReadOnlyCollection<IReadOnlyNode> IReadOnlyNodeGraph.AllNodes => Nodes;
    IReadOnlyNode IReadOnlyNodeGraph.OutputNode => OutputNode;

    public void AddNode(Node node)
    {
        if (Nodes.Contains(node))
        {
            return;
        }
        
        node.ConnectionsChanged += ResetCache;
        _nodes.Add(node);
        ResetCache();
    }

    public void RemoveNode(Node node)
    {
        if (!Nodes.Contains(node))
        {
            return;
        }

        node.ConnectionsChanged -= ResetCache;
        _nodes.Remove(node);
        ResetCache();
    }

    public Queue<IReadOnlyNode> CalculateExecutionQueue(IReadOnlyNode outputNode)
    {
        return new Queue<IReadOnlyNode>(CalculateExecutionQueueInternal(outputNode));
    }
    
    private ImmutableList<IReadOnlyNode> CalculateExecutionQueueInternal(IReadOnlyNode outputNode)
    {
        return cachedExecutionList ??= GraphUtils.CalculateExecutionQueue(outputNode).ToImmutableList();
    }

    void IReadOnlyNodeGraph.AddNode(IReadOnlyNode node) => AddNode((Node)node);

    void IReadOnlyNodeGraph.RemoveNode(IReadOnlyNode node) => RemoveNode((Node)node);

    public void Dispose()
    {
        foreach (var node in Nodes)
        {
            node.Dispose();
        }
    }

    public bool TryTraverse(Action<IReadOnlyNode> action)
    {
        if(OutputNode == null) return false;
        
        var queue = CalculateExecutionQueueInternal(OutputNode);
        
        foreach (var node in queue)
        {
            action(node);
        }
        
        return true;
    }

    public void Execute(RenderContext context)
    {
        if (OutputNode == null) return;
        if(!CanExecute()) return;

        var queue = CalculateExecutionQueueInternal(OutputNode);
        
        foreach (var node in queue)
        {
            if (node is Node typedNode)
            {
                if(typedNode.IsDisposed) continue;
                
                typedNode.ExecuteInternal(context);
            }
            else
            {
                node.Execute(context);
            }
        }
    }
    
    private bool CanExecute()
    {
        foreach (var node in Nodes)
        {
            if (node.IsDisposed)
            {
                return false;
            }
        }

        return true;
    }
    
    private void ResetCache()
    {
        cachedExecutionList = null;
    }
}
