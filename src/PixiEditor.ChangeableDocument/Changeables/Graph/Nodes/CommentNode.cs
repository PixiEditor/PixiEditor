using Drawie.Backend.Core.ColorsImpl;
using Drawie.Numerics;

using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Comment")]
public class CommentNode : Node
{
    public InputProperty<string> CommentName { get; }
    public InputProperty<string> CommentText { get; }
    public InputProperty<VecI> Size { get; }
    public InputProperty<VecI> Offset { get; }
    public InputProperty<Color> Color { get; }

    public CommentNode()
    {
        CommentName = CreateInput("Name", "NAME", "");
        CommentText = CreateInput("Text", "TEXT", "");
        Size = CreateInput("Size", "SIZE", new VecI(100, 100)).WithRules(v => v.Min(VecI.One));
        Offset = CreateInput("Offset", "OFFSET", new VecI(32, 132));
        Color = CreateInput<Color>("Color", "COMMENT_WINDOW_COLOR", Colors.Gray);
    }

    protected override void OnExecute(RenderContext context)
    {
        
    }

    public override Node CreateCopy()
    {
        return new CommentNode();
    }
}
