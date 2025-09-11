using Drawie.Backend.Core.Shaders;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Utility;

[NodeInfo("RepeatEnd")]
[PairNode(typeof(RepeatNodeStart), "RepeatZone", false)]
public class RepeatNodeEnd : Node, IPairNode, IExecutionFlowNode
{
    public InputProperty<object> Input { get; }
    public OutputProperty<object> Output { get; }

    public Guid OtherNode { get; set; }

    private RepeatNodeStart startNode;

    public RepeatNodeEnd()
    {
        Input = CreateInput<object>("Input", "INPUT", null);
        Output = CreateOutput<object>("Output", "OUTPUT", null);
    }

    protected override void OnExecute(RenderContext context)
    {
        if (OtherNode == Guid.Empty || startNode == null)
        {
            startNode = FindStartNode();
            OtherNode = startNode?.Id ?? Guid.Empty;
            if (OtherNode == Guid.Empty)
            {
                return;
            }
        }

        Output.Value = Input.Value;
    }

    public override Node CreateCopy()
    {
        return new RepeatNodeEnd();
    }

    private RepeatNodeStart FindStartNode()
    {
        RepeatNodeStart startNode = null;
        int nestingCount = 0;
        TraverseBackwards(node =>
        {
            if (node is RepeatNodeEnd && node != this)
            {
                nestingCount++;
            }
            if (node is RepeatNodeStart leftNode && nestingCount == 0)
            {
                startNode = leftNode;
                return false;
            }
            if (node is RepeatNodeStart)
            {
                nestingCount--;
            }

            return true;
        });

        return startNode;
    }

    public HashSet<IReadOnlyNode> HandledNodes => CalculateHandledNodes();

    private HashSet<IReadOnlyNode> CalculateHandledNodes()
    {
        HashSet<IReadOnlyNode> handled = new();

        startNode = FindStartNode();

        int nestingCount = 0;
        var queue = GraphUtils.CalculateExecutionQueue(this, false, property => property.Connection.Node != startNode);

        foreach (var node in queue)
        {
            if (node is RepeatNodeStart && node != this)
            {
                nestingCount++;
            }
            if (node is RepeatNodeEnd leftNode && nestingCount == 0)
            {
                if (leftNode == this)
                {
                    break;
                }
                nestingCount--;
            }

            if (node != this && node != startNode)
            {
                handled.Add(node);
            }
        }

        return handled;
    }
}
