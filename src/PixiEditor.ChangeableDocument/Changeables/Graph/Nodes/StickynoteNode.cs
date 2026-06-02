using Drawie.Backend.Core.ColorsImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("StickyNote")]
public class StickyNoteNode : Node
{
    public const string TextPropertyName = "Text";
    public const string SizePropertyName = "Size";
    public const string ColorPropertyName = "Color";

    public InputProperty<string> Text { get; }
    public InputProperty<VecI> Size { get; }
    public InputProperty<Color> Color { get; }

    public StickyNoteNode()
    {
        Text = CreateInput(TextPropertyName, "TEXT_LABEL", "");
        Size = CreateInput(SizePropertyName, "SIZE", new VecI(200, 120)).WithRules(v => v.Min(VecI.One));
        Color = CreateInput<Color>(ColorPropertyName, "STICKYNOTE_COLOR", new Color(255, 235, 153, 255));
    }

    public override Node CreateCopy() => new StickyNoteNode();

    protected override void OnExecute(RenderContext context)
    {
    }
}
