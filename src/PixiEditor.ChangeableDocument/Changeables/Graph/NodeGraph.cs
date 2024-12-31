using System.Collections;
using System.Diagnostics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public class NodeGraph : IReadOnlyNodeGraph, IDisposable
{
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

        _nodes.Add(node);
    }

    public void RemoveNode(Node node)
    {
        if (!Nodes.Contains(node))
        {
            return;
        }

        _nodes.Remove(node);
    }

    public Queue<IReadOnlyNode> CalculateExecutionQueue(IReadOnlyNode outputNode)
    {
        return GraphUtils.CalculateExecutionQueue(outputNode);
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
        
        var queue = CalculateExecutionQueue(OutputNode);
        
        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            action(node);
        }
        
        return true;
    }

    public void Execute(RenderContext context)
    {
        if (OutputNode == null) return;
        if(!CanExecute()) return;

        var queue = CalculateExecutionQueue(OutputNode);
        
        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            
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
}
