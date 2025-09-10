using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Matrix;

[NodeInfo("DecomposeMatrix")]
public class DecomposeMatrixNode : Node
{
    public FuncInputProperty<Float3x3, ShaderFuncContext> Matrix { get; }

    public FuncOutputProperty<Float1, ShaderFuncContext> ScaleX { get; }
    public FuncOutputProperty<Float1, ShaderFuncContext> SkewX { get; }
    public FuncOutputProperty<Float1, ShaderFuncContext> TransX { get; }
    public FuncOutputProperty<Float1, ShaderFuncContext> SkewY { get; }
    public FuncOutputProperty<Float1, ShaderFuncContext> ScaleY { get; }
    public FuncOutputProperty<Float1, ShaderFuncContext> TransY { get; }
    public FuncOutputProperty<Float1, ShaderFuncContext> Persp0 { get; }
    public FuncOutputProperty<Float1, ShaderFuncContext> Persp1 { get; }
    public FuncOutputProperty<Float1, ShaderFuncContext> Persp2 { get; }

    public DecomposeMatrixNode()
    {
        Matrix = CreateFuncInput<Float3x3, ShaderFuncContext>("Matrix", "MATRIX",
            new Float3x3("") { ConstantValue = Matrix3X3.Identity });
        ScaleX = CreateFuncOutput<Float1, ShaderFuncContext>("ScaleX", "SCALE_X", context => context.GetValue(Matrix).M11);
        ScaleY = CreateFuncOutput<Float1, ShaderFuncContext>("ScaleY", "SCALE_Y", context => context.GetValue(Matrix).M22);
        SkewX = CreateFuncOutput<Float1, ShaderFuncContext>("SkewX", "SKEW_X", context => context.GetValue(Matrix).M12);
        SkewY = CreateFuncOutput<Float1, ShaderFuncContext>("SkewY", "SKEW_Y", context => context.GetValue(Matrix).M21);
        TransX = CreateFuncOutput<Float1, ShaderFuncContext>("TranslateX", "TRANSLATE_X", context => context.GetValue(Matrix).M13);
        TransY = CreateFuncOutput<Float1, ShaderFuncContext>("TranslateY", "TRANSLATE_Y", context => context.GetValue(Matrix).M23);
        Persp0 = CreateFuncOutput<Float1, ShaderFuncContext>("Perspective0", "PERSPECTIVE_0", context => context.GetValue(Matrix).M31);
        Persp1 = CreateFuncOutput<Float1, ShaderFuncContext>("Perspective1", "PERSPECTIVE_1", context => context.GetValue(Matrix).M32);
        Persp2 = CreateFuncOutput<Float1, ShaderFuncContext>("Perspective2", "PERSPECTIVE_2", context => context.GetValue(Matrix).M33);
    }

    protected override void OnExecute(RenderContext context)
    {

    }

    public override Node CreateCopy()
    {
        return new DecomposeMatrixNode();
    }
}
