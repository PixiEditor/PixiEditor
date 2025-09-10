using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Utility;

[NodeInfo("RepeatStart")]
[PairNode(typeof(RepeatNodeEnd), "RepeatZone", true)]
public class RepeatNodeStart : Node
{
    public FuncInputProperty<int, RepeatFuncContext> Iterations { get; }
    public FuncInputProperty<object, RepeatFuncContext> Input { get; }
    public FuncOutputProperty<int, RepeatFuncContext> CurrentIteration { get; }
    public FuncOutputProperty<object, RepeatFuncContext> Output { get; }

    public RepeatNodeStart()
    {
        Iterations = CreateFuncInput<int, RepeatFuncContext>("Iterations", "ITERATIONS", 1);
        Input = CreateFuncInput<object, RepeatFuncContext>("Input", "INPUT", null);
        CurrentIteration = CreateFuncOutput<int, RepeatFuncContext>("CurrentIteration", "CURRENT_ITERATION", GetIteration);
        Output = CreateFuncOutput<object, RepeatFuncContext>("Output", "OUTPUT", GetOutput);
    }

    protected override void OnExecute(RenderContext context)
    {
        RepeatFuncContext funcContext = new RepeatFuncContext(1);
    }

    private int GetIteration(RepeatFuncContext context)
    {
        return context.CurrentIteration;
    }

    private object GetOutput(RepeatFuncContext context)
    {
        return null;
    }

    public override Node CreateCopy()
    {
        return new RepeatNodeStart();
    }
}
