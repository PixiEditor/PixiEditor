using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Shaders.Generation;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using PixiEditor.Common;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Math")]
public class MathNode : Node
{
    public FuncOutputProperty<Float1> Result { get; }

    public InputProperty<MathNodeMode> Mode { get; }

    public SyncedTypeInputProperty TestSyncedItem1 { get; }
    public SyncedTypeInputProperty TestSyncedItem2 { get; }

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
        TestSyncedItem1 = CreateSyncedTypeInput("TestSyncedItem1", "TEST_SYNCED_ITEM_1", null);
        TestSyncedItem2 = CreateSyncedTypeInput("TestSyncedItem2", "TEST_SYNCED_ITEM_2", TestSyncedItem1);
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
                MathNodeMode.Compare => ShaderMath.Compare(x, y, z),
                MathNodeMode.Power => ShaderMath.Power(x, y),
                MathNodeMode.Logarithm => ShaderMath.Log(x, y),
                MathNodeMode.NaturalLogarithm => ShaderMath.LogE(x),
                MathNodeMode.Root => ShaderMath.Root(x, y),
                MathNodeMode.InverseRoot => ShaderMath.InverseRoot(x, y),
                MathNodeMode.Fraction => ShaderMath.Fraction(x),
                MathNodeMode.Absolute => ShaderMath.Abs(x),
                MathNodeMode.Negate => ShaderMath.Negate(x),
                MathNodeMode.Floor => ShaderMath.Floor(x),
                MathNodeMode.Ceil => ShaderMath.Ceil(x),
                MathNodeMode.Round => ShaderMath.Round(x),
                MathNodeMode.Modulo => ShaderMath.Modulo(x, y),
                MathNodeMode.Min => ShaderMath.Min(x, y),
                MathNodeMode.Max => ShaderMath.Max(x, y),
                MathNodeMode.Step => ShaderMath.Step(x, y),
                MathNodeMode.SmoothStep => ShaderMath.SmoothStep(x, y, z),
            };

            if (Clamp.Value)
            {
                result = ShaderMath.Clamp(result, (Float1)0, (Float1)1);
            }

            return context.NewFloat1(result);
        }

        var xConst = (double)x.GetConstant();
        var yConst = (double)y.GetConstant();
        var zConst = (double)z.GetConstant();
        
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
            MathNodeMode.Compare => Math.Abs(xConst - yConst) <= zConst ? 1 : 0,
            MathNodeMode.Power => Math.Pow(xConst, yConst),
            MathNodeMode.Logarithm => Math.Log(xConst, yConst),
            MathNodeMode.NaturalLogarithm => Math.Log(xConst),
            MathNodeMode.Root => Math.Pow(xConst, 1.0 / yConst),
            MathNodeMode.InverseRoot => 1.0 / Math.Pow(xConst, 1.0 / yConst),
            MathNodeMode.Fraction => 1.0 / xConst,
            MathNodeMode.Absolute => Math.Abs(xConst),
            MathNodeMode.Negate => -xConst,
            MathNodeMode.Floor => Math.Floor(xConst),
            MathNodeMode.Ceil => Math.Ceiling(xConst),
            MathNodeMode.Round => Math.Round(xConst),
            MathNodeMode.Modulo => xConst % yConst,
            MathNodeMode.Min => Math.Min(xConst, yConst),
            MathNodeMode.Max => Math.Max(xConst, yConst),
            MathNodeMode.Step => xConst > yConst ? 1 : 0,
            MathNodeMode.SmoothStep => MathEx.SmoothStep(xConst, yConst, zConst),
        };
        
        if (Clamp.Value)
        {
            constValue = Math.Clamp(constValue, 0, 1);
        }
            
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
