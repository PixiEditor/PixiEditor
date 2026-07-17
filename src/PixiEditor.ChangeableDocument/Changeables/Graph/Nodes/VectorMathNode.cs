using System.ComponentModel;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Shaders.Generation;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Numerics;
using PixiEditor.Common;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("VectorMath")]
public class VectorMathNode : Node
{
    public FuncOutputProperty<Float1> ResultFloat1 { get; }
    public FuncOutputProperty<Float2> Result { get; }

    public InputProperty<VectorMathMode> Mode { get; }

    public FuncInputProperty<Float2> X { get; }

    public FuncInputProperty<Float2> Y { get; }

    public FuncInputProperty<Float2> Z { get; }
    public FuncInputProperty<Float1> S { get; }

    public VectorMathNode()
    {
        ResultFloat1 = CreateFuncOutput<Float1>(nameof(ResultFloat1), "RESULT", CalculateF1);
        Result = CreateFuncOutput<Float2>(nameof(Result), "RESULT", Calculate);
        Mode = CreateInput(nameof(Mode), "MATH_MODE", VectorMathMode.Add);
        X = CreateFuncInput<Float2>(nameof(X), "X", VecD.Zero);
        Y = CreateFuncInput<Float2>(nameof(Y), "Y", VecD.Zero);
        Z = CreateFuncInput<Float2>(nameof(Z), "Z", VecD.Zero);
        S = CreateFuncInput<Float1>(nameof(S), "S", 0d);
    }

    private Float2 Calculate(FuncContext context)
    {
        var (x, y, z, s) = GetValues(context);

        if (context.HasContext)
        {
            Expression? result = Mode.Value switch
            {
                VectorMathMode.Add => ShaderMath.Add(x, y),
                VectorMathMode.Subtract => ShaderMath.Subtract(x, y),
                VectorMathMode.Multiply => ShaderMath.Multiply(x, y),
                VectorMathMode.Divide => ShaderMath.Divide(x, y),
                VectorMathMode.Sin => ShaderMath.Sin(x),
                VectorMathMode.Cos => ShaderMath.Cos(x),
                VectorMathMode.Tan => ShaderMath.Tan(x),
                VectorMathMode.Power => ShaderMath.Power(x, y),
                VectorMathMode.Fraction => ShaderMath.Fraction(x),
                VectorMathMode.Absolute => ShaderMath.Abs(x),
                VectorMathMode.Floor => ShaderMath.Floor(x),
                VectorMathMode.Ceil => ShaderMath.Ceil(x),
                VectorMathMode.Round => ShaderMath.Round(x),
                VectorMathMode.Modulo => ShaderMath.Modulo(x, y),
                VectorMathMode.Negate => ShaderMath.Negate(x),
                VectorMathMode.Min => ShaderMath.Min(x, y),
                VectorMathMode.Max => ShaderMath.Max(x, y),
                VectorMathMode.MultiplyAdd => ShaderMath.MultiplyAdd(x, y, z),
                VectorMathMode.Scale => ShaderMath.Scale(x, s),
                VectorMathMode.Normalize => ShaderMath.Normalize(x),
                VectorMathMode.Sign => ShaderMath.Sign(x),
                VectorMathMode.Wrap => ShaderMath.Wrap(x, y, z),
                VectorMathMode.Snap => ShaderMath.Snap(x, y),
                _ => context.NewFloat2(new Float1(""), new Float1(""))
            };

            return context.NewFloat2(result);
        }

        var xConstRaw = x.GetConstant();
        var yConstRaw = y.GetConstant();
        var zConstRaw = z.GetConstant();
        var sConstRaw = s.GetConstant();

        VecD xConst = xConstRaw is VecD xV ? xV : VecD.Zero;
        VecD yConst = yConstRaw is VecD yV ? yV : VecD.Zero;
        VecD zConst = zConstRaw is VecD zV ? zV : VecD.Zero;
        double sConst = sConstRaw is double sD ? sD : 0d;

        xConst = TryCast(xConstRaw, xConst);
        yConst = TryCast(yConstRaw, yConst);
        zConst = TryCast(zConstRaw, zConst);

        if (sConstRaw is not double)
        {
            try
            {
                sConst = Convert.ToDouble(sConstRaw);
            }
            catch
            {
                sConst = 0d;
            }
        }

        VecD constValue;
        switch (Mode.Value)
        {
            case VectorMathMode.Add:
                constValue = xConst + yConst;
                break;
            case VectorMathMode.Subtract:
                constValue = xConst - yConst;
                break;
            case VectorMathMode.Multiply:
                constValue = new VecD(xConst.X * yConst.X, xConst.Y * yConst.Y);
                break;
            case VectorMathMode.Divide:
                constValue =new VecD(
                    yConst.X != 0 ? xConst.X / yConst.X : 0,
                    yConst.Y != 0 ? xConst.Y / yConst.Y : 0
                );
                break;
            case VectorMathMode.Sin:
                constValue = new VecD(Math.Sin(xConst.X), Math.Sin(xConst.Y));
                break;
            case VectorMathMode.Cos:
                constValue = new VecD(Math.Cos(xConst.X), Math.Cos(xConst.Y));
                break;
            case VectorMathMode.Tan:
                constValue = new VecD(Math.Tan(xConst.X), Math.Tan(xConst.Y));
                break;
            case VectorMathMode.Power:
                constValue = new VecD(Math.Pow(xConst.X, yConst.X), Math.Pow(xConst.Y, yConst.Y));
                break;
            case VectorMathMode.Fraction:
                constValue = new VecD(xConst.X - Math.Floor(xConst.X), xConst.Y - Math.Floor(xConst.Y));
                break;
            case VectorMathMode.Absolute:
                constValue = new VecD(Math.Abs(xConst.X), Math.Abs(xConst.Y));
                break;
            case VectorMathMode.Negate:
                constValue = new VecD(-xConst.X, -xConst.Y);
                break;
            case VectorMathMode.Floor:
                constValue = new VecD(Math.Floor(xConst.X), Math.Floor(xConst.Y));
                break;
            case VectorMathMode.Ceil:
                constValue = new VecD(Math.Ceiling(xConst.X), Math.Ceiling(xConst.Y));
                break;
            case VectorMathMode.Round:
                constValue = new VecD(Math.Round(xConst.X), Math.Round(xConst.Y));
                break;
            case VectorMathMode.Modulo:
                constValue = new VecD(Modulo(xConst.X, yConst.X), Modulo(xConst.Y, yConst.Y));
                break;
            case VectorMathMode.Min:
                constValue = new VecD(Math.Min(xConst.X, yConst.X), Math.Min(xConst.Y, yConst.Y));
                break;
            case VectorMathMode.Max:
                constValue = new VecD(Math.Max(xConst.X, yConst.X), Math.Max(xConst.Y, yConst.Y));
                break;
            case VectorMathMode.MultiplyAdd:
                constValue = new VecD(xConst.X * yConst.X + zConst.X, xConst.Y * yConst.Y + zConst.Y);
                break;
            case VectorMathMode.Dot:
                constValue = new VecD(xConst.X * yConst.X + xConst.Y * yConst.Y);
                break;
            case VectorMathMode.Distance:
                constValue = new VecD(Math.Sqrt(Math.Pow(xConst.X - yConst.X, 2) + Math.Pow(xConst.Y - yConst.Y, 2)));
                break;
            case VectorMathMode.Length:
                constValue = new VecD(Math.Sqrt(xConst.X * xConst.X + xConst.Y * xConst.Y));
                break;
            case VectorMathMode.Scale:
                constValue = new VecD(xConst.X * sConst, xConst.Y * sConst);
                break;
            case VectorMathMode.Normalize:
                var length = Math.Sqrt(xConst.X * xConst.X + xConst.Y * xConst.Y);
                constValue = length > 0 ? new VecD(xConst.X / length, xConst.Y / length) : VecD.Zero;
                break;
            case VectorMathMode.Sign:
                constValue = new VecD(Math.Sign(xConst.X), Math.Sign(xConst.Y));
                break;
            case VectorMathMode.Wrap:
                constValue = Wrap(xConst, yConst, zConst);
                break;
            case VectorMathMode.Snap:
                constValue = new VecD(Math.Floor(xConst.X / yConst.X) * yConst.X,
                    Math.Floor(xConst.Y / yConst.Y) * yConst.Y);
                break;
            default:
                constValue = xConst + yConst;
                break;
        }

        return new Float2(string.Empty) { ConstantValue = constValue };
    }

    private Float1 CalculateF1(FuncContext context)
    {
        var (x, y, _, _) = GetValues(context);

        if (context.HasContext)
        {
            Expression? result = Mode.Value switch
            {
                VectorMathMode.Dot => ShaderMath.Dot(x, y),
                VectorMathMode.Cross => ShaderMath.Cross(x, y),
                VectorMathMode.Distance => ShaderMath.Distance(x, y),
                VectorMathMode.Length => ShaderMath.Length(x),
                _ => null
            };

            return context.NewFloat1(result ?? new Float1(""){ ConstantValue = 0d });
        }

        var xConstRaw = x.GetConstant();
        var yConstRaw = y.GetConstant();

        VecD xConst = xConstRaw is VecD xV ? xV : VecD.Zero;
        VecD yConst = yConstRaw is VecD yV ? yV : VecD.Zero;

        xConst = TryCast(xConstRaw, xConst);
        yConst = TryCast(yConstRaw, yConst);

        double constValue;
        switch (Mode.Value)
        {
            case VectorMathMode.Dot:
                constValue = xConst.X * yConst.X + xConst.Y * yConst.Y;
                break;
            case VectorMathMode.Distance:
                constValue = Math.Sqrt(Math.Pow(xConst.X - yConst.X, 2) + Math.Pow(xConst.Y - yConst.Y, 2));
                break;
            case VectorMathMode.Length:
                constValue = Math.Sqrt(xConst.X * xConst.X + xConst.Y * xConst.Y);
                break;
            default:
                constValue = 0d;
                break;
        }

        return new Float1(string.Empty) { ConstantValue = constValue };
    }

    private static VecD TryCast(object raw, VecD aConst)
    {
        if (raw is not VecD)
        {
            if (raw is VecI xI)
            {
                aConst = new VecD(xI.X, xI.Y);
            }
            else if (raw is double xD)
            {
                aConst = new VecD(xD);
            }
            else
            {
                try
                {
                    aConst = new VecD(Convert.ToDouble(raw));
                }
                catch
                {
                    aConst = VecD.Zero;
                }
            }
        }

        return aConst;
    }

    private (Float2 xConst, Float2 y, Float2 z, Float1 s) GetValues(FuncContext context)
    {
        return (context.GetValue(X), context.GetValue(Y), context.GetValue(Z), context.GetValue(S));
    }


    protected override void OnExecute(RenderContext context)
    {
    }


    public override Node CreateCopy() => new VectorMathNode();

    private static double Modulo(double a, double b)
    {
        return (a % b + b) % b;
    }

    public static VecD Wrap(VecD value, VecD min, VecD max)
    {
        return new VecD(
            WrapScalar(value.X, min.X, max.X),
            WrapScalar(value.Y, min.Y, max.Y)
        );
    }

    private static double WrapScalar(double x, double min, double max)
    {
        double range = max - min;
        if (range == 0)
            return min; // or 0 depending on your convention

        return x - range * Math.Floor((x - min) / range);
    }
}

public enum VectorMathMode
{
    [Description("MATH_ADD")]
    Add,
    [Description("MATH_SUBTRACT")]
    Subtract,
    [Description("MULTIPLY")]
    Multiply,
    [Description("DIVIDE")]
    Divide,
    [Description("MATH_MULTIPLY_ADD")]
    MultiplyAdd,
    [Description("MATH_DOT")]
    Dot,
    [Description("MATH_CROSS")]
    Cross,
    [Description("DISTANCE")]
    Distance,
    [Description("LENGTH")]
    Length,
    [Description("MATH_SCALE")]
    Scale,
    [Description("MATH_NORMALIZE")]
    Normalize,
    [Description("ABSOLUTE")]
    Absolute,
    [Description("MATH_POWER")]
    Power,
    [Description("MATH_SIGN")]
    Sign,
    [Description("MIN")]
    Min,
    [Description("MAX")]
    Max,
    [Description("ROUND")]
    Round,
    [Description("FLOOR")]
    Floor,
    [Description("CEIL")]
    Ceil,
    [Description("FRACTION")]
    Fraction,
    [Description("MODULO")]
    Modulo,
    [Description("MATH_WRAP")]
    Wrap,
    [Description("MATH_SNAP")]
    Snap,
    [Description("SIN")]
    Sin,
    [Description("COS")]
    Cos,
    [Description("TAN")]
    Tan,
    [Description("NEGATE")]
    Negate
}

public static class VectorMathModeExtensions
{
    public static bool UsesYValue(this VectorMathMode mode) =>
        mode != VectorMathMode.Sin &&
        mode != VectorMathMode.Cos &&
        mode != VectorMathMode.Tan &&
        mode != VectorMathMode.Fraction &&
        mode != VectorMathMode.Absolute &&
        mode != VectorMathMode.Negate &&
        mode != VectorMathMode.Floor &&
        mode != VectorMathMode.Ceil &&
        mode != VectorMathMode.Round &&
        mode != VectorMathMode.Length &&
        mode != VectorMathMode.Normalize &&
        mode != VectorMathMode.Sign &&
        mode != VectorMathMode.Scale;


    public static bool UsesZValue(this VectorMathMode mode) =>
        mode is VectorMathMode.MultiplyAdd or
            VectorMathMode.Wrap;

    public static bool UsesSValue(this VectorMathMode mode) =>
        mode is VectorMathMode.Scale;

    public static (string x, string y, string z) GetNaming(this VectorMathMode mode) => mode switch
    {
        _ => ("X", "Y", "Z")
    };

    public static bool ProducesVector(this VectorMathMode mode) =>
        mode != VectorMathMode.Dot &&
        mode != VectorMathMode.Distance &&
        mode != VectorMathMode.Length &&
        mode != VectorMathMode.Cross;

        public static bool ProducesDouble(this VectorMathMode mode) => !mode.ProducesVector();
}
