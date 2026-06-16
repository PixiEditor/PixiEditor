using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Calculations;

[NodeInfo("Angle")]
public class AngleNode : Node
{
    public FuncInputProperty<Float2> Vector { get; }
    public FuncOutputProperty<Float1> Radians { get; }
    public FuncOutputProperty<Float1> Degrees { get; }

    public AngleNode()
    {
        Vector = CreateFuncInput(nameof(Vector), "VECTOR", new Float2(""));
        Radians = CreateFuncOutput<Float1>(nameof(Radians), "RADIANS", Calculate);
        Degrees = CreateFuncOutput<Float1>(nameof(Degrees), "DEGREES", CalculateDegrees);
    }

   private Float1 Calculate(FuncContext context)
    {
        var vector = context.GetValue(Vector);

        if (context.HasContext)
        {
            var result = ShaderMath.Atan2(vector.Y, vector.X);

            return context.NewFloat1(result);
        }

        var vecConst = (VecD)vector.GetConstant();

        var constValue = Math.Atan2(vecConst.Y, vecConst.X);

        return new Float1(string.Empty) { ConstantValue = constValue };
    }

    private Float1 CalculateDegrees(FuncContext context)
    {
         var vector = context.GetValue(Vector);

        if (context.HasContext)
        {
            var result = ShaderMath.Atan2Deg(vector.Y, vector.X);

            return context.NewFloat1(result);
        }

        var vecConst = (VecD)vector.GetConstant();

        var constValue = Math.Atan2(vecConst.Y, vecConst.X) * (180 / Math.PI);

        return new Float1(string.Empty) { ConstantValue = constValue };
    }

    protected override void OnExecute(RenderContext context)
    {

    }

    public override Node CreateCopy()
    {
        return new AngleNode();
    }
}
