using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Text;

[NodeInfo("CharacterPosition")]
public class TextIndexOfNode : Node
{
    public OutputProperty<int> FirstIndex { get; }
    
    public OutputProperty<int> LastIndex { get; }
    
    public InputProperty<bool> MatchCase { get; }
    
    public InputProperty<string?> Text { get; }
    
    public InputProperty<string?> SearchText { get; }

    public TextIndexOfNode()
    {
        FirstIndex = CreateOutput("FirstIndex", "FIRST_POSITION", -1);
        LastIndex = CreateOutput("LastIndex", "LAST_POSITION", -1);
        
        MatchCase = CreateInput("MatchCase", "MATCH_CASE", false);
        Text = CreateInput("Text", "TEXT", string.Empty);
        SearchText = CreateInput("SearchText", "SEARCH_TEXT", string.Empty);
    }

    protected override void OnExecute(RenderContext context)
    {
        var comparisonMode = MatchCase.Value
            ? StringComparison.InvariantCulture
            : StringComparison.InvariantCultureIgnoreCase;

        var text = Text.Value;
        var searchText = SearchText.Value;

        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(searchText))
        {
            FirstIndex.Value = -1;
            LastIndex.Value = -1;
            
            return;
        }

        FirstIndex.Value = text.IndexOf(searchText, comparisonMode);
        LastIndex.Value = text.LastIndexOf(searchText, comparisonMode);
    }

    public override Node CreateCopy() =>
        new TextIndexOfNode();
}
