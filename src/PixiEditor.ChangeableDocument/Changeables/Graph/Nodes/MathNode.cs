using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Math", "MATH_NODE", Category = "NUMBERS")]
public class MathNode : Node
{
    public FuncOutputProperty<Float1> Result { get; }

    public InputProperty<MathNodeMode> Mode { get; }
    
    public InputProperty<bool> Clamp { get; }

    public FuncInputProperty<Float1> X { get; }
    
    public FuncInputProperty<Float1> Y { get; }
    
    public MathNode()
    {
        Result = CreateFuncOutput<Float1>(nameof(Result), "RESULT", Calculate);
        Mode = CreateInput(nameof(Mode), "MATH_MODE", MathNodeMode.Add);
        Clamp = CreateInput(nameof(Clamp), "CLAMP", false);
        X = CreateFuncInput<Float1>(nameof(X), "X", 0d);
        Y = CreateFuncInput<Float1>(nameof(Y), "Y", 0d);
    }

    private Float1 Calculate(FuncContext context)
    {
        var (x, y) = GetValues(context);

        if (context.HasContext)
        {
            var result = Mode.Value switch
            {
                MathNodeMode.Add => ShaderMath.Add(x, y),
                MathNodeMode.Subtract => ShaderMath.Subtract(x, y),
                MathNodeMode.Multiply => ShaderMath.Multiply(x, y),
                MathNodeMode.Divide => ShaderMath.Divide(x, y),
                MathNodeMode.Sin => ShaderMath.Sin(x),
                MathNodeMode.Cos => ShaderMath.Cos(x),
                MathNodeMode.Tan => ShaderMath.Tan(x),
            };

            if (Clamp.Value)
            {
                result = ShaderMath.Clamp(result, (Float1)0, (Float1)1);
            }

            return context.NewFloat1(result);
        }

        var xConst = x.ConstantValue;
        var yConst = y.ConstantValue;
            
        var constValue = Mode.Value switch
        {
            MathNodeMode.Add => xConst + yConst,
            MathNodeMode.Subtract => xConst - yConst,
            MathNodeMode.Multiply => xConst * yConst,
            MathNodeMode.Divide => xConst / yConst,
            MathNodeMode.Sin => Math.Sin(xConst),
            MathNodeMode.Cos => Math.Cos(xConst),
            MathNodeMode.Tan => Math.Tan(xConst),
        };
            
        return new Float1(string.Empty) { ConstantValue = constValue };
    }

    private (Float1 xConst, Float1 y) GetValues(FuncContext context)
    {
        return (context.GetValue(X), context.GetValue(Y));
    }


    protected override Texture? OnExecute(RenderingContext context)
    {
        return null;
    }


    public override Node CreateCopy() => new MathNode();
}
