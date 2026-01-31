using Drawie.Backend.Core.Shaders.Generation;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Utility;

[NodeInfo("Switch")]
public class SwitchNode : Node
{
    public InputProperty<bool> Condition { get; }
    public SyncedTypeInputProperty InputTrue { get; }
    public SyncedTypeInputProperty InputFalse { get; }
    public SyncedTypeOutputProperty Output { get; }

    public SwitchNode()
    {
        Condition = CreateInput<bool>("Condition", "CONDITION", false);
        InputTrue = CreateSyncedTypeInput("InputTrue", "ON_TRUE", null);
        InputFalse = CreateSyncedTypeInput("InputFalse", "ON_FALSE", InputTrue);
        Output = CreateSyncedTypeOutput("Output", "RESULT", InputTrue);
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
