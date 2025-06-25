using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Matrix;

[NodeInfo("ComposeMatrix")]
public class ComposeMatrixNode : Node
{
    public FuncInputProperty<Float3x3> MatrixInput { get; }
    public FuncInputProperty<Float1> ScaleX { get; }
    public FuncInputProperty<Float1> SkewX { get; }
    public FuncInputProperty<Float1> TransX { get; }
    public FuncInputProperty<Float1> SkewY { get; }
    public FuncInputProperty<Float1> ScaleY { get; }
    public FuncInputProperty<Float1> TransY { get; }
    public FuncInputProperty<Float1> Persp0 { get; }
    public FuncInputProperty<Float1> Persp1 { get; }
    public FuncInputProperty<Float1> Persp2 { get; }

    public FuncOutputProperty<Float3x3> Matrix { get; }

    public ComposeMatrixNode()
    {
        MatrixInput = CreateFuncInput<Float3x3>("MatrixInput", "INPUT_MATRIX",
            new Float3x3("") { ConstantValue = Matrix3X3.Identity });

        ScaleX = CreateFuncInput<Float1>("ScaleX", "SCALE_X", 1.0f);
        ScaleY = CreateFuncInput<Float1>("ScaleY", "SCALE_Y", 1.0f);
        SkewX = CreateFuncInput<Float1>("SkewX", "SKEW_X", 0.0f);
        SkewY = CreateFuncInput<Float1>("SkewY", "SKEW_Y", 0.0f);
        TransX = CreateFuncInput<Float1>("TranslateX", "TRANSLATE_X", 0.0f);
        TransY = CreateFuncInput<Float1>("TranslateY", "TRANSLATE_Y", 0.0f);
        Persp0 = CreateFuncInput<Float1>("Perspective0", "PERSPECTIVE_0", 0.0f);
        Persp1 = CreateFuncInput<Float1>("Perspective1", "PERSPECTIVE_1", 0.0f);
        Persp2 = CreateFuncInput<Float1>("Perspective2", "PERSPECTIVE_2", 1.0f);

        Matrix = CreateFuncOutput<Float3x3>("Matrix", "MATRIX", ComposeMatrix);
    }

    private Float3x3 ComposeMatrix(FuncContext context)
    {
        if (context.HasContext)
        {
            var composed = context.NewFloat3x3(
                context.GetValue(ScaleX),
                context.GetValue(SkewX),
                context.GetValue(TransX),
                context.GetValue(SkewY),
                context.GetValue(ScaleY),
                context.GetValue(TransY),
                context.GetValue(Persp0),
                context.GetValue(Persp1),
                context.GetValue(Persp2)
            );

            if (MatrixInput.Connection != null)
            {
                return context.NewFloat3x3(ShaderMath.PostConcat(context.GetValue(MatrixInput), composed));
            }

            return composed;
        }

        var mtx = new Float3x3("")
        {
            ConstantValue
                = new Matrix3X3(
                    (float)(context.GetValue(ScaleX).GetConstant() as double? ?? 1.0),
                    (float)(context.GetValue(SkewX).GetConstant() as double? ?? 0.0),
                    (float)(context.GetValue(TransX).GetConstant() as double? ?? 0.0),
                    (float)(context.GetValue(SkewY).GetConstant() as double? ?? 0.0),
                    (float)(context.GetValue(ScaleY).GetConstant() as double? ?? 1.0),
                    (float)(context.GetValue(TransY).GetConstant() as double? ?? 0.0),
                    (float)(context.GetValue(Persp0).GetConstant() as double? ?? 0.0),
                    (float)(context.GetValue(Persp1).GetConstant() as double? ?? 0.0),
                    (float)(context.GetValue(Persp2).GetConstant() as double? ?? 1.0))
        };

        if (MatrixInput.Connection != null)
        {
            mtx.ConstantValue = mtx.ConstantValue.PostConcat(
                (context.GetValue(MatrixInput).ConstantValue as Matrix3X3?) ?? Matrix3X3.Identity);
        }

        return mtx;
    }

    protected override void OnExecute(RenderContext context)
    {
    }

    public override Node CreateCopy()
    {
        return new ComposeMatrixNode();
    }
}
