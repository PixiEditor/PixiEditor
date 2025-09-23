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
            startNode = FindStartNode(out _);
            return startNode?.Id ?? Guid.Empty;
        }
        set
        {
            // no op, the start node is found dynamically
        }
    }

    internal RepeatNodeStart startNode;

    public HashSet<IReadOnlyNode> HandledNodes => CalculateHandledNodes();

    public RepeatNodeEnd()
    {
        Input = CreateInput<object>("Input", "INPUT", null);
        Output = CreateOutput<object>("Output", "OUTPUT", null);
    }

    protected override void OnExecute(RenderContext context)
    {
        if (startNode == null && OtherNode == Guid.Empty)
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

    internal RepeatNodeStart FindStartNode(out List<IReadOnlyNode> reversedCalculationQueue)
    {
        RepeatNodeStart startNode = null;
        int nestingCount = 0;

        var queue = GraphUtils.CalculateExecutionQueue(this);

        int nestingLevel = 0;
        var reversedQueue = queue.Reverse().ToList();
        foreach (var node in reversedQueue)
        {
            if (node == this)
            {
                continue;
            }

            if (node is RepeatNodeEnd)
            {
                nestingLevel++;
            }

            if (node is RepeatNodeStart leftNode)
            {
                if (nestingLevel == 0)
                {
                    startNode = leftNode;
                    break;
                }

                nestingLevel--;
            }
        }

        reversedCalculationQueue = reversedQueue;
        return startNode;
    }

    private HashSet<IReadOnlyNode> CalculateHandledNodes()
    {
        HashSet<IReadOnlyNode> handled = new();

        startNode = FindStartNode(out var calculationQueue);

        int nestingCount = 0;

        bool withinPair = false;
        foreach (var node in calculationQueue)
        {
            if (node == this)
            {
                withinPair = true;
                continue;
            }

            if (node is RepeatNodeEnd)
            {
                nestingCount++;
            }

            if (node is RepeatNodeStart leftNode)
            {
                if (nestingCount == 0)
                {
                    if (leftNode == startNode)
                    {
                        break;
                    }
                }
                else
                {
                    nestingCount--;
                }
            }

            if (withinPair)
            {
                handled.Add(node);
            }
        }

        return handled;
    }
}
