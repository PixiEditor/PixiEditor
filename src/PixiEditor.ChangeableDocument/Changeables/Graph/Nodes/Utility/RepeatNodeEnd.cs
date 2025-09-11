using Drawie.Backend.Core.Shaders;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Utility;

[NodeInfo("RepeatEnd")]
[PairNode(typeof(RepeatNodeStart), "RepeatZone", false)]
public class RepeatNodeEnd : Node, IPairNode
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

        int iterations = startNode.Iterations.Value;
        var queue = GraphUtils.CalculateExecutionQueue(this, false, (input => input.Connection?.Node != startNode));

        for (int i = 0; i < iterations; i++)
        {
            startNode.CurrentIteration.Value = i;
            foreach (var node in queue)
            {
                if (node is RepeatNodeStart or RepeatNodeEnd)
                {
                    continue;
                }

                node.Execute(context);
            }

            startNode.Output.Value = Input.Value;
        }
        startNode.CurrentIteration.Value = 0;
        if (iterations <= 0)
        {
            Output.Value = startNode.Input.Value;
        }
        else
        {
            Output.Value = Input.Value;
        }
    }

    private object GetOutput(ShaderFuncContext context)
    {
        return null;
    }

    public override Node CreateCopy()
    {
        return new RepeatNodeEnd();
    }

    private RepeatNodeStart FindStartNode()
    {
        RepeatNodeStart startNode = null;
        TraverseBackwards(node =>
        {
            if (node is RepeatNodeStart leftNode)
            {
                startNode = leftNode;
                return false;
            }

            return true;
        });

        return startNode;
    }
}
