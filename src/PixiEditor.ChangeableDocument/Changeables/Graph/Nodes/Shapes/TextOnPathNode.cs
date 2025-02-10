using Drawie.Backend.Core.Vector;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes;

[NodeInfo("TextOnPath")]
public class TextOnPathNode : Node
{
    public InputProperty<TextVectorData> TextData { get; }
    public InputProperty<ShapeVectorData> PathData { get; }

    public OutputProperty<TextVectorData> Output { get; }

    private VectorPath lastPath;

    public TextOnPathNode()
    {
        TextData = CreateInput<TextVectorData>("Text", "TEXT_LABEL", null);
        PathData = CreateInput<ShapeVectorData>("Path", "SHAPE_LABEL", null);

        Output = CreateOutput<TextVectorData>("Output", "TEXT_LABEL", null);
    }

    protected override void OnExecute(RenderContext context)
    {
        var textData = TextData.Value;
        var pathData = PathData.Value;

        if (textData == null || pathData == null || !textData.IsValid() || !pathData.IsValid())
        {
            Output.Value = null;
            return;
        }

        var cloned = (TextVectorData)textData.Clone();

        lastPath?.Dispose();
        lastPath = pathData.ToPath();
        lastPath.Transform(pathData.TransformationMatrix);

        cloned.Path = lastPath;

        Output.Value = cloned;
    }

    public override Node CreateCopy()
    {
        return new TextOnPathNode();
    }
}
