using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Evaluator;

[NodeInfo("ColorEvaluatorLeft")]
[PairNode(typeof(ColorEvaluatorRightNode), "ColorEvaluatorZone", true)]
public class ColorEvaluatorLeftNode : Node
{
    public override string DisplayName { get; set; } = "BEGIN_COLOR_EVALUATOR";

    public FuncOutputProperty<VecD> Position { get; }

    public ColorEvaluatorLeftNode()
    {
        Position = CreateFuncOutput("Position", "POSITION", c => c.Position);
    }
    
    protected override Surface? OnExecute(RenderingContext context)
    {
        return null;
    }

    public override Node CreateCopy() => new ColorEvaluatorLeftNode();
}
