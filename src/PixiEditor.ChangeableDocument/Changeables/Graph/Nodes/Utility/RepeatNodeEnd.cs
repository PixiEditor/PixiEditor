using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Shaders.Generation;
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

    public Guid OtherNode
    {
        get
        {
            startNode = FindStartNode();
            return startNode?.Id ?? Guid.Empty;
        }
        set
        {
           // no op, the start node is found dynamically
        }
    }

    private RepeatNodeStart startNode;

    public HashSet<IReadOnlyNode> HandledNodes => CalculateHandledNodes();

    public RepeatNodeEnd()
    {
        Input = CreateInput<object>("Input", "INPUT", null);
        Output = CreateOutput<object>("Output", "OUTPUT", null);
    }

    protected override void OnExecute(RenderContext context)
    {
        if (OtherNode == Guid.Empty)
        {
            return;
        }

        if (startNode.Iterations.Value == 0)
        {
            Output.Value = DefaultOfType(Input.Value);
            return;
        }

        Output.Value = Input.Value;
    }

    private object DefaultOfType(object? val)
    {
        if (val == null) return null;
        var type = val.GetType();
        if (type.IsValueType) return Activator.CreateInstance(type)!;
        if (val is Delegate del)
        {
            var result = del.DynamicInvoke(ShaderFuncContext.NoContext);
            if (result is ShaderExpressionVariable expressionVariable)
            {
                return DefaultOfType(expressionVariable.GetConstant());
            }
        }

        return null;
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
