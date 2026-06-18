using Drawie.Backend.Core.ColorsImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Comment")]
public class CommentNode : Node
{
    public const string TextPropertyName = "Text";
    public const string SizePropertyName = "Size";
    public const string ColorPropertyName = "Color";

    public InputProperty<string> CommentText { get; }
    public InputProperty<VecI> Size { get; }
    public InputProperty<Color> Color { get; }

    public CommentNode()
    {
        CommentText = CreateInput(TextPropertyName, "TEXT", "");
        Size = CreateInput(SizePropertyName, "SIZE", new VecI(200, 120)).WithRules(v => v.Min(VecI.One));
        Color = CreateInput<Color>(ColorPropertyName, "COMMENT_WINDOW_COLOR", Colors.LightGray);
    }

    protected override void OnExecute(RenderContext context)
    {
    }

    public override Node CreateCopy() => new CommentNode();
}
