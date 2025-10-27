using Drawie.Backend.Core.Shaders.Generation;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
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
        InputTrue = CreateInput<object>("InputTrue", "ON_TRUE", null);
        InputFalse = CreateInput<object>("InputFalse", "ON_FALSE", null);
        Output = CreateOutput<object>("Output", "RESULT", null);
    }

    protected override void OnExecute(RenderContext context)
    {
        Output.Value = Condition.Value ? InputTrue.Value : InputFalse.Value;
        if(Output.Value is Delegate del)
        {
            Output.Value = del.DynamicInvoke(FuncContext.NoContext);
            if(Output.Value is ShaderExpressionVariable expr)
            {
                Output.Value = expr.GetConstant();
            }
        }
    }

    public override Node CreateCopy()
    {
        return new SwitchNode();
    }
}
