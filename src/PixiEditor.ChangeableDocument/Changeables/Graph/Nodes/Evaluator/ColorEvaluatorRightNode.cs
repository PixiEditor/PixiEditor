using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Evaluator;

[NodeInfo("ColorEvaluatorRight", "FINISH_COLOR_EVALUATOR", PickerName = "")]
[PairNode(typeof(ColorEvaluatorLeftNode), "ColorEvaluatorZone")]
public class ColorEvaluatorRightNode : Node
{
    public FuncOutputProperty<Half4> Output { get; }

    public FuncInputProperty<Half4> Input { get; }

    public ColorEvaluatorRightNode()
    {
        Output = CreateFuncOutput("Output", "COLOR", c => Input.Value(c));
        Input = CreateFuncInput<Half4>("Input", "COLOR", Colors.Black);
    }
    
    protected override Texture? OnExecute(RenderingContext context)
    {
        return null;
    }

    public override Node CreateCopy() => new ColorEvaluatorRightNode();
}
