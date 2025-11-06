using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Utility;

[NodeInfo("Equals")]
public class EqualsNode : Node
{
    public InputProperty<object> A { get; }
    public InputProperty<object> B { get; }
    public OutputProperty<bool> Result { get; }

    public EqualsNode()
    {
        A = CreateInput<object>("A", "A", null);
        B = CreateInput<object>("B", "B", null);
        Result = CreateOutput<bool>("Result", "RESULT", false);
    }

    protected override void OnExecute(RenderContext context)
    {
        Result.Value = Equals(A.Value, B.Value);
    }

    public override Node CreateCopy()
    {
        return new EqualsNode();
    }
}
