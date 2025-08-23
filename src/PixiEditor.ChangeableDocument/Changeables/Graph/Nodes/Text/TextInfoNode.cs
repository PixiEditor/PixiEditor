using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Text;

[NodeInfo("TextInfo")]
public class TextInfoNode : Node
{
    public OutputProperty<int> Length { get; }
    
    public OutputProperty<int> LineCount { get; }
    
    public InputProperty<string> Text { get; }

    public TextInfoNode()
    {
        Length = CreateOutput("Length", "TEXT_LENGTH", 0);
        LineCount = CreateOutput("LineCount", "TEXT_LINE_COUNT", 0);
        
        Text = CreateInput("Text", "TEXT", string.Empty);
    }
    
    protected override void OnExecute(RenderContext context)
    {
        var text = Text.Value;
        
        Length.Value = text.Length;
        LineCount.Value = text.AsSpan().Count('\n');
    }

    public override Node CreateCopy() =>
        new TextInfoNode();
}
