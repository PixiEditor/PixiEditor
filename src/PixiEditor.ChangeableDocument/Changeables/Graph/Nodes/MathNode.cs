using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Shaders.Generation;
using Drawie.Backend.Core.Shaders.Generation.Expressions;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Math")]
public class MathNode : Node
{
    public FuncOutputProperty<Float1> Result { get; }

    public InputProperty<MathNodeMode> Mode { get; }
    
    public InputProperty<bool> Clamp { get; }

    public FuncInputProperty<Float1> X { get; }
    
    public FuncInputProperty<Float1> Y { get; }
    
    public FuncInputProperty<Float1> Z { get; }
    
    public MathNode()
    {
        Result = CreateFuncOutput<Float1>(nameof(Result), "RESULT", Calculate);
        Mode = CreateInput(nameof(Mode), "MATH_MODE", MathNodeMode.Add);
        Clamp = CreateInput(nameof(Clamp), "CLAMP", false);
        X = CreateFuncInput<Float1>(nameof(X), "X", 0d);
        Y = CreateFuncInput<Float1>(nameof(Y), "Y", 0d);
        Z = CreateFuncInput<Float1>(nameof(Z), "Z", 0d);
    }

    private Float1 Calculate(FuncContext context)
    {
        var (x, y, z) = GetValues(context);

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
                MathNodeMode.GreaterThan => ShaderMath.GreaterThan(x, y),
                MathNodeMode.GreaterThanOrEqual => ShaderMath.GreaterThanOrEqual(x, y),
                MathNodeMode.LessThan => ShaderMath.LessThan(x, y),
                MathNodeMode.LessThanOrEqual => ShaderMath.LessThanOrEqual(x, y),
                MathNodeMode.Compare => ShaderMath.Compare(x, y, z)
            };

            if (Clamp.Value)
            {
                result = ShaderMath.Clamp(result, (Float1)0, (Float1)1);
            }

            return context.NewFloat1(result);
        }

        var xConst = x.ConstantValue;
        var yConst = y.ConstantValue;
        var zConst = z.ConstantValue;
        
        var constValue = Mode.Value switch
        {
            MathNodeMode.Add => xConst + yConst,
            MathNodeMode.Subtract => xConst - yConst,
            MathNodeMode.Multiply => xConst * yConst,
            MathNodeMode.Divide => xConst / yConst,
            MathNodeMode.Sin => Math.Sin(xConst),
            MathNodeMode.Cos => Math.Cos(xConst),
            MathNodeMode.Tan => Math.Tan(xConst),
            MathNodeMode.GreaterThan => xConst > yConst ? 1 : 0,
            MathNodeMode.GreaterThanOrEqual => xConst >= yConst ? 1 : 0,
            MathNodeMode.LessThan => xConst < yConst ? 1 : 0,
            MathNodeMode.LessThanOrEqual => xConst <= yConst ? 1 : 0,
            MathNodeMode.Compare => Math.Abs(xConst - yConst) < zConst ? 1 : 0
        };
            
        return new Float1(string.Empty) { ConstantValue = constValue };
    }

    private (Float1 xConst, Float1 y, Float1 z) GetValues(FuncContext context)
    {
        return (context.GetValue(X), context.GetValue(Y), context.GetValue(Z));
    }


    protected override void OnExecute(RenderContext context)
    {
    }


    public override Node CreateCopy() => new MathNode();
}
