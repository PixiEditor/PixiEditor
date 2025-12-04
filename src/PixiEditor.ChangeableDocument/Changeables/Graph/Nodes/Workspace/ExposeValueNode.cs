using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Workspace;

[NodeInfo("ExposeValue")]
public class ExposeValueNode : Node
{
    public InputProperty<string> Name { get; }
    public InputProperty<object?> Value { get; }

    public ExposeValueNode()
    {
        Name = CreateInput<string>("Name", "NAME", "");
        Value = CreateInput<object?>("Input", "INPUT", null);
    }

    protected override void OnExecute(RenderContext context)
    {
        // no op
    }

    public override Node CreateCopy()
    {
        return new ExposeValueNode();
    }
}
