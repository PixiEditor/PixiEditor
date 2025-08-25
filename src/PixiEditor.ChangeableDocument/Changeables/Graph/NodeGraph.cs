﻿using System.Collections.Immutable;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public class NodeGraph : IReadOnlyNodeGraph, IDisposable
{
    private Dictionary<IReadOnlyNode, ImmutableList<IReadOnlyNode>?> cachedExecutionList;

    private readonly List<Node> _nodes = new();
    public IReadOnlyCollection<Node> Nodes => _nodes;
    public IReadOnlyDictionary<Guid, Node> NodeLookup => nodeLookup;
    public Node? OutputNode => CustomOutputNode ?? Nodes.OfType<OutputNode>().FirstOrDefault();
    public Node? CustomOutputNode { get; set; }

    private Dictionary<Guid, Node> nodeLookup = new();

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
        nodeLookup[node.Id] = node;
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
        nodeLookup.Remove(node.Id);
        ResetCache();
    }

    public Node? FindNode(Guid guid)
    {
        return nodeLookup.GetValueOrDefault(guid);
    }

    public T? FindNode<T>(Guid guid) where T : Node
    {
        return nodeLookup.TryGetValue(guid, out Node? node) && node is T typedNode ? typedNode : null;
    }

    public Queue<IReadOnlyNode> CalculateExecutionQueue(IReadOnlyNode outputNode)
    {
        return new Queue<IReadOnlyNode>(CalculateExecutionQueueInternal(outputNode));
    }

    private ImmutableList<IReadOnlyNode> CalculateExecutionQueueInternal(IReadOnlyNode outputNode)
    {
        var cached = this.cachedExecutionList?.GetValueOrDefault(outputNode);
        if (cached != null)
        {
            return cached;
        }

        var calculated = GraphUtils.CalculateExecutionQueue(outputNode).ToImmutableList();
        cachedExecutionList ??= new Dictionary<IReadOnlyNode, ImmutableList<IReadOnlyNode>?>();
        cachedExecutionList[outputNode] = calculated;
        return calculated;
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
        if (OutputNode == null) return false;
        return TryTraverse(OutputNode, action);
    }

    public bool TryTraverse(IReadOnlyNode end, Action<IReadOnlyNode> action)
    {
        var queue = CalculateExecutionQueueInternal(end);

        foreach (var node in queue)
        {
            action(node);
        }

        return true;
    }

    public void Execute(IReadOnlyNode end, RenderContext context)
    {
        if (end == null) return;
        if (!CanExecute()) return;

        var queue = CalculateExecutionQueueInternal(end);

        foreach (var node in queue)
        {
            if (node is Node typedNode)
            {
                if (typedNode.IsDisposed) continue;

                typedNode.ExecuteInternal(context);
            }
            else
            {
                node.Execute(context);
            }
        }
    }

    public void Execute(RenderContext context) => Execute(OutputNode, context);

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

    public int GetCacheHash()
    {
        HashCode hash = new();
        foreach (var node in Nodes)
        {
            hash.Add(node.GetCacheHash());
        }

        return hash.ToHashCode();
    }
}
