using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("BlackboardVariableValue")]
public class BlackboardVariableValueNode : Node
{
    public InputProperty<string> VariableName { get; }
    public OutputProperty<object> Value { get; }

    public BlackboardVariableValueNode()
    {
        VariableName = CreateInput("VariableName", "VARIABLE_NAME", string.Empty);
        Value = CreateOutput<object>("Value", "VALUE", null);
    }

    protected override void OnExecute(RenderContext context)
    {
        var variable = context.Graph.Blackboard.GetVariable(VariableName.Value);
        if (variable != null)
        {
            Value.Value = variable.Value;
        }
        else
        {
            Value.Value = null;
        }
    }

    public override Node CreateCopy()
    {
        return new BlackboardVariableValueNode();
    }
}
