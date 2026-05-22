using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
[NodeInfo("Concatenation")]

public class ConcatenationNode : Node
{
    public InputProperty<string> String1 { get; }
    public InputProperty<string> String2 { get; }
    public OutputProperty<string> Output { get; }

    
    public ConcatenationNode() {
        String1 = CreateInput("String1", "TEXT_LABEL", "");
        String2 = CreateInput("String2", "TEXT_LABEL", "");
        Output = CreateOutput("Output", "TEXT_LABEL", "");

    }

    protected override void OnExecute(RenderContext context)
    {
        var Text1 = String1.Value;
        var Text2 = String2.Value;
        if (Text1 == null || Text2 == null) {
            Output.Value = string.Empty;
            return;
        }
        Output.Value = Text1 + Text2;
        return;
    }

    public override Node CreateCopy()
    {
        return new ConcatenationNode();
    }
}