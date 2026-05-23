using Drawie.Backend.Core.ColorsImpl;
using Drawie.Numerics;

using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Comment")]
public class CommentNode : Node
{
    public const string TextPropertyName = "Text";
    public const string SizePropertyName = "Size";
    public const string OffsetPropertyName = "Offset";
    public const string ColorPropertyName = "Color";

    public InputProperty<string> CommentName { get; }
    public InputProperty<string> CommentText { get; }
    public InputProperty<VecI> Size { get; }
    public InputProperty<VecI> Offset { get; }
    public InputProperty<Color> Color { get; }

    public CommentNode()
    {
        CommentText = CreateInput(TextPropertyName, "TEXT", "");
        Size = CreateInput(SizePropertyName, "SIZE", new VecI(100, 100)).WithRules(v => v.Min(VecI.One));
        Offset = CreateInput(OffsetPropertyName, "OFFSET", new VecI(32, 250));
        Color = CreateInput<Color>(ColorPropertyName, "COMMENT_WINDOW_COLOR", Colors.Gray);
    }

    protected override void OnExecute(RenderContext context)
    {
        
    }

    public override Node CreateCopy()
    {
        return new CommentNode();
    }
}
