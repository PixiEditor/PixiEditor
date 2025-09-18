using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Text;

[NodeInfo("SliceText")]
public class SliceTextNode : Node
{
    public OutputProperty<string> SlicedText { get; }
    
    public InputProperty<bool> UseLength { get; }
    
    public InputProperty<string?> Text { get; }
    
    public InputProperty<int> Index { get; }
    
    public InputProperty<int> Length { get; }
    
    public SliceTextNode()
    {
        SlicedText = CreateOutput("SlicedText", "TEXT", string.Empty);
        Text = CreateInput("Text", "TEXT", string.Empty);
        UseLength = CreateInput("UseLength", "TEXT_SLICE_USE_LENGTH", true);
        
        Index = CreateInput("Index", "INDEX_START_AT", 0)
            .WithRules(x => x.Min(0));
        
        Length = CreateInput("Length", "LENGTH", 1)
            .WithRules(x => x.Min(0));
    }

    protected override void OnExecute(RenderContext context)
    {
        var text = Text.Value;

        if (text == null)
        {
            SlicedText.Value = string.Empty;
            return;
        }

        var startIndex = Math.Clamp(Index.Value, 0, text.Length);

        if (!UseLength.Value)
        {
            SlicedText.Value = text.Substring(startIndex);
            return;
        }

        var length = Math.Clamp(Length.Value, 0, text.Length - startIndex);
        
        SlicedText.Value = text.Substring(startIndex, length);
    }

    public override Node CreateCopy() =>
        new SliceTextNode();
}
