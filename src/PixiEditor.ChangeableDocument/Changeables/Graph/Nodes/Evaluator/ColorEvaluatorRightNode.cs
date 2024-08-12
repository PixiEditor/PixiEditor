using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Evaluator;

[NodeInfo("ColorEvaluatorRight", "FINISH_COLOR_EVALUATOR", PickerName = "")]
[PairNode(typeof(ColorEvaluatorLeftNode), "ColorEvaluatorZone")]
public class ColorEvaluatorRightNode : Node
{
    public FuncOutputProperty<Color> Output { get; }

    public FuncInputProperty<Color> Input { get; }

    public ColorEvaluatorRightNode()
    {
        Output = CreateFuncOutput("Output", "COLOR", c => Input.Value(c));
        Input = CreateFuncInput("Input", "COLOR", Colors.Black);
    }
    
    protected override Surface? OnExecute(RenderingContext context)
    {
        return null;
    }

    public override Node CreateCopy() => new ColorEvaluatorRightNode();
}
