using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Utility;

[NodeInfo("Switch")]
public class SwitchNode : Node
{
    public InputProperty<bool> Condition { get; }
    public InputProperty<object> InputTrue { get; }
    public InputProperty<object> InputFalse { get; }

    public OutputProperty<object> Output { get; }

    public SwitchNode()
    {
        Condition = CreateInput<bool>("Condition", "CONDITION", false);
        InputTrue = CreateInput<object>("InputTrue", "IN_TRUE", null);
        InputFalse = CreateInput<object>("InputFalse", "IN_FALSE", null);
        Output = CreateOutput<object>("Output", "OUT", null);
    }

    protected override void OnExecute(RenderContext context)
    {
        Output.Value = Condition.Value ? InputTrue.Value : InputFalse.Value;
    }

    public override Node CreateCopy()
    {
        return new SwitchNode();
    }
}
