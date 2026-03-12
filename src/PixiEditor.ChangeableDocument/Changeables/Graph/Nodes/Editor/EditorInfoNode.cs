using Drawie.Backend.Core.ColorsImpl;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Editor;

[NodeInfo("EditorInfo")]
public class EditorInfoNode : Node
{
    public OutputProperty<Color> PrimaryColor { get; }
    public OutputProperty<Color> SecondaryColor { get; }

    public EditorInfoNode()
    {
        PrimaryColor = CreateOutput<Color>("PrimaryColor", "PRIMARY_COLOR", Colors.Black);
        SecondaryColor = CreateOutput<Color>("SecondaryColor", "SECONDARY_COLOR", Colors.White);
    }

    protected override void OnExecute(RenderContext context)
    {
        PrimaryColor.Value = context.EditorData.PrimaryColor;
        SecondaryColor.Value = context.EditorData.SecondaryColor;
    }

    public override Node CreateCopy()
    {
        return new EditorInfoNode();
    }
}
