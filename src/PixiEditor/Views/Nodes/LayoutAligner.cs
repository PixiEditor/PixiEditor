using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.Models.Handlers;

namespace PixiEditor.Views.Nodes;

internal class LayoutAligner
{
    public double VerticalSpacing { get; set; } = 50;
    public double HorizontalSpacing { get; set; } = 50;
    public INodeGraphHandler NodeGraph { get; set; }

    public Dictionary<INodeHandler, VecD> NodeSizes { get; set; }

    public LayoutGroup RootGroup { get; set; }

    Dictionary<INodeHandler, RectD> boundsMap = new();

    public LayoutAligner(INodeGraphHandler nodeGraph, Dictionary<INodeHandler, RectD> nodeSizes)
    {
        NodeGraph = nodeGraph;
        NodeSizes = nodeSizes.ToDictionary(x => x.Key, x => x.Value.Size);
    }

    public void SplitToGroups()
    {
        RootGroup = SplitToGroups(NodeGraph.OutputNode, NodeGraph.OutputNode);
    }

    public void Align()
    {
        AlignNodes(RootGroup.SourceNode);
    }

    private void AlignNodes(INodeHandler sourceNode)
    {
        if (sourceNode == null) return;
        foreach (var node in NodeGraph.AllNodes)
        {
            boundsMap[node] = new RectD(sourceNode.PositionBindable, NodeSizes[node]);
        }

        sourceNode.TraverseBackwards((node, previousNode, previousConnection) =>
        {
            if (previousNode == null)
            {
                if (sourceNode == null) return true;

                boundsMap[node] =
                    new RectD(VecD.Zero, boundsMap[node].Size);
                node.PositionBindable = boundsMap[node].TopLeft;
                return true;
            }

            RectD previousBounds = boundsMap[previousNode];
            RectD bounds = boundsMap[node];

            int verticalCount = previousNode.Inputs.Count(x => x.ConnectedOutput != null);
            double totalHeightRequired = 0;
            double startY = 0;

            int indexOfVertical = 0;
            bool found = false;
            foreach (var input in previousNode.Inputs)
            {
                if (input.ConnectedOutput == null) continue;

                if (input != previousConnection && !found)
                {
                    indexOfVertical++;
                }
                else if (!found)
                {
                    startY = totalHeightRequired;
                    found = true;
                }

                totalHeightRequired += boundsMap[input.ConnectedOutput.Node].Height;
            }

            double shift = -totalHeightRequired / 2f + bounds.Height / 2f;
            double moveYby = shift + startY + VerticalSpacing * indexOfVertical;

            double x = previousBounds.X - previousBounds.Width - HorizontalSpacing;
            double y = verticalCount == 1 ? previousBounds.Y : previousBounds.Y + moveYby;

            bounds = new RectD(x, y, bounds.Width, bounds.Height);

            node.PositionBindable = bounds.TopLeft;
            boundsMap[node] = bounds;
            return true;
        });
    }

    private LayoutGroup SplitToGroups(INodeHandler startNode, INodeHandler sourceNode, LayoutGroup parent = null)
    {
        var group = new LayoutGroup { Nodes = [startNode], Parent = parent, SourceNode = sourceNode };

        startNode.TraverseBackwards((node, connection) =>
        {
            if (group.Nodes.Contains(node))
            {
                return true;
            }

            group.Nodes.Add(node);

            if (HasMultipleDistinctConnections(node, out var distinctNodes))
            {
                group.Children ??= new List<LayoutGroup>();
                foreach (var distinctNode in distinctNodes)
                {
                    group.Children.Add(SplitToGroups(distinctNode, node, group));
                }
            }

            return true;
        });

        return group;
    }

    private RectD GetBounds(List<INodeHandler> nodes)
    {
        double minX = double.MaxValue;
        double minY = double.MaxValue;
        double maxX = double.MinValue;
        double maxY = double.MinValue;

        foreach (var node in nodes)
        {
            var position = boundsMap[node].TopLeft;
            minX = Math.Min(minX, position.X);
            minY = Math.Min(minY, position.Y);
            maxX = Math.Max(maxX, position.X + NodeSizes[node].X);
            maxY = Math.Max(maxY, position.Y + NodeSizes[node].Y);
        }

        return new RectD(minX, minY, maxX - minX, maxY - minY);
    }

    private bool HasMultipleDistinctConnections(INodeHandler node, out List<INodeHandler> distinctNodes)
    {
        List<INodeHandler> connectedNodes = new();
        foreach (var input in node.Inputs)
        {
            if (input.ConnectedOutput != null && !connectedNodes.Contains(input.ConnectedOutput.Node))
            {
                connectedNodes.Add(input.ConnectedOutput.Node);
            }
        }

        distinctNodes = connectedNodes;
        return connectedNodes.Count > 1;
    }

    public static Queue<LayoutGroup> CalculateExecutionQueue(LayoutGroup outputNode)
    {
        var finalQueue = new HashSet<LayoutGroup>();
        var queueNodes = new Queue<LayoutGroup>();
        queueNodes.Enqueue(outputNode);

        while (queueNodes.Count > 0)
        {
            var node = queueNodes.Dequeue();

            if (finalQueue.Contains(node))
            {
                continue;
            }

            bool canAdd = true;

            if (node.Children == null)
            {
                finalQueue.Add(node);
                continue;
            }

            foreach (var input in node.Children)
            {
                if (finalQueue.Contains(input))
                {
                    continue;
                }

                canAdd = false;

                if (finalQueue.Contains(input))
                {
                    finalQueue.Remove(input);
                    finalQueue.Add(input);
                }

                if (!queueNodes.Contains(input))
                {
                    queueNodes.Enqueue(input);
                }
            }

            if (canAdd)
            {
                finalQueue.Add(node);
            }
            else
            {
                queueNodes.Enqueue(node);
            }
        }

        return new Queue<LayoutGroup>(finalQueue);
    }
}

public class LayoutGroup
{
    public LayoutGroup Parent { get; set; }
    public List<LayoutGroup> Children { get; set; }
    public List<INodeHandler> Nodes { get; set; }
    public INodeHandler SourceNode { get; set; }
}
