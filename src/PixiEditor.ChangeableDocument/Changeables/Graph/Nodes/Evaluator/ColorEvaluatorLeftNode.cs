using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Evaluator;

[NodeInfo("ColorEvaluatorLeft", "BEGIN_COLOR_EVALUATOR", PickerName = "COLOR_EVALUATOR_NODE_PAIR")]
[PairNode(typeof(ColorEvaluatorRightNode), "ColorEvaluatorZone", true)]
public class ColorEvaluatorLeftNode : Node
{
    public FuncOutputProperty<Float2> Position { get; }

    public ColorEvaluatorLeftNode()
    {
        Position = CreateFuncOutput("Position", "UV", c => c.Position);
    }
    
    protected override Texture? OnExecute(RenderingContext context)
    {
        return null;
    }

    public override Node CreateCopy() => new ColorEvaluatorLeftNode();
}
