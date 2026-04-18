using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Arrays;

[NodeInfo("ArrayLength")]
public class ArrayLengthNode : Node
{
    public InputProperty<object[]> Array { get; }
    public OutputProperty<int> Length { get; }

    public ArrayLengthNode()
    {
        Array = CreateInput<object[]>("Array", "ARRAY", null);
        Length = CreateOutput<int>("Length", "LENGTH", 0);
    }

    protected override void OnExecute(RenderContext context)
    {
        if (Array.Value != null)
        {
            Length.Value = Array.Value.Length;
        }
        else
        {
            Length.Value = 0;
        }
    }

    public override Node CreateCopy()
    {
        return new ArrayLengthNode();
    }
}
