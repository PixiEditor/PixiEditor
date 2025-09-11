using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Utility;

[NodeInfo("RepeatStart")]
[PairNode(typeof(RepeatNodeEnd), "RepeatZone", true)]
public class RepeatNodeStart : Node, IExecutionFlowNode
{
    public InputProperty<int> Iterations { get; }
    public InputProperty<object> Input { get; }
    public OutputProperty<int> CurrentIteration { get; }
    public OutputProperty<object> Output { get; }

    public RepeatNodeStart()
    {
        Iterations = CreateInput<int>("Iterations", "ITERATIONS", 1);
        Input = CreateInput<object>("Input", "INPUT", null);
        CurrentIteration = CreateOutput<int>("CurrentIteration", "CURRENT_ITERATION", 0);
        Output = CreateOutput<object>("Output", "OUTPUT", null);
    }

    protected override void OnExecute(RenderContext context)
    {
        Output.Value = Input.Value;
        CurrentIteration.Value = 0;
    }

    public override Node CreateCopy()
    {
        return new RepeatNodeStart();
    }

    public HashSet<IReadOnlyNode> HandledNodes => CalculateHandledNodes();

    private HashSet<IReadOnlyNode> CalculateHandledNodes()
    {
        HashSet<IReadOnlyNode> handled = new();

        TraverseForwards(node =>
        {
            if (node is RepeatNodeEnd)
            {
                return false;
            }

            if (node != this)
            {
                handled.Add(node);
            }

            return true;
        });

        return handled;
    }
}
