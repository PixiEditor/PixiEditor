using Drawie.Backend.Core.Shaders.Generation.Expressions;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Animable;

[NodeInfo("Easing")]
public class EasingNode : Node
{
    public FuncInputProperty<Float1> Value { get; }
    public InputProperty<EasingType> Easing { get; }
    public FuncOutputProperty<Float1> Output { get; }

    public EasingNode()
    {
        Value = CreateFuncInput<Float1>("Value", "VALUE", 0.0);
        Easing = CreateInput("EasingType", "EASING_TYPE", EasingType.Linear);
        Output = CreateFuncOutput<Float1>("Output", "OUTPUT", Evaluate);
    }

    protected override void OnExecute(RenderContext context)
    {
    }

    public Float1 Evaluate(FuncContext context)
    {
        var x = context.GetValue(Value);
        if (!context.HasContext)
        {
            return new Float1(string.Empty) { ConstantValue = EvalCpu(x.ConstantValue) };
        }

        return context.NewFloat1(GetExpression(context));
    }

    private double EvalCpu(double x)
    {
        const double c1 = 1.70158;
        const double c2 = c1 * 1.525;
        const double c3 = c1 + 1;
        const double c4 = (2 * Math.PI) / 3;
        const double c5 = (2 * Math.PI) / 4.5;

        EasingType easing = Easing.Value;
        return easing switch
        {
            EasingType.Linear => x,
            EasingType.InSine => 1 - Math.Cos((x * Math.PI) / 2),
            EasingType.OutSine => Math.Sin((x * Math.PI) / 2),
            EasingType.InOutSine => -(Math.Cos(Math.PI * x) - 1) / 2,
            EasingType.InQuad => x * x,
            EasingType.OutQuad => x * (2 - x),
            EasingType.InOutQuad => x < 0.5 ? 2 * x * x : 1 - Math.Pow(-2 * x + 2, 2) / 2,
            EasingType.InCubic => x * x * x,
            EasingType.OutCubic => 1 - Math.Pow(1 - x, 3),
            EasingType.InOutCubic => x < 0.5 ? 4 * x * x * x : 1 - Math.Pow(-2 * x + 2, 3) / 2,
            EasingType.InQuart => x * x * x * x,
            EasingType.OutQuart => 1 - Math.Pow(1 - x, 4),
            EasingType.InOutQuart => x < 0.5 ? 8 * x * x * x * x : 1 - Math.Pow(-2 * x + 2, 4) / 2,
            EasingType.InQuint => x * x * x * x * x,
            EasingType.OutQuint => 1 - Math.Pow(1 - x, 5),
            EasingType.InOutQuint => x < 0.5 ? 16 * x * x * x * x * x : 1 - Math.Pow(-2 * x + 2, 5) / 2,
            EasingType.InExpo => x == 0 ? 0 : Math.Pow(2, 10 * x - 10),
            EasingType.OutExpo => x == 1 ? 1 : 1 - Math.Pow(2, -10 * x),
            EasingType.InOutExpo => x == 0 ? 0 :
                x == 1 ? 1 :
                x < 0.5 ? Math.Pow(2, 20 * x - 10) / 2 : (2 - Math.Pow(2, -20 * x + 10)) / 2,
            EasingType.InCirc => 1 - Math.Sqrt(1 - Math.Pow(x, 2)),
            EasingType.OutCirc => Math.Sqrt(1 - Math.Pow(x - 1, 2)),
            EasingType.InOutCirc => x < 0.5
                ? (1 - Math.Sqrt(1 - Math.Pow(2 * x, 2))) / 2
                : (Math.Sqrt(1 - Math.Pow(-2 * x + 2, 2)) + 1) / 2,
            EasingType.InBack => c3 * x * x * x - c1 * x * x,
            EasingType.OutBack => 1 + c3 * Math.Pow(x - 1, 3) + c1 * Math.Pow(x - 1, 2),
            EasingType.InOutBack => x < 0.5
                ? (Math.Pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2)) / 2
                : (Math.Pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2,
            EasingType.InElastic => x switch
            {
                0 => 0,
                1 => 1,
                _ => -Math.Pow(2, 10 * x - 10) * Math.Sin((x * 10 - 10.75) * c4)
            },
            EasingType.OutElastic => x switch
            {
                0 => 0,
                1 => 1,
                _ => Math.Pow(2, -10 * x) * Math.Sin((x * 10 - 0.75) * c4) + 1
            },
            EasingType.InOutElastic => x == 0 ? 0 :
                x == 1 ? 1 :
                x < 0.5 ? -(Math.Pow(2, 20 * x - 10) * Math.Sin((20 * x - 11.125) * c5)) / 2 :
                (Math.Pow(2, -20 * x + 10) * Math.Sin((20 * x - 11.125) * c5)) / 2 + 1,
            EasingType.InBounce => 1 - OutBounce(1 - x),
            EasingType.OutBounce => OutBounce(x),
            EasingType.InOutBounce => x < 0.5 ? (1 - OutBounce(1 - 2 * x)) / 2 : (1 + OutBounce(2 * x - 1)) / 2,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private double OutBounce(double x)
    {
        const double n1 = 7.5625;
        const double d1 = 2.75;

        if (x < 1 / d1)
        {
            return n1 * x * x;
        }

        if (x < 2 / d1)
        {
            return n1 * (x -= 1.5 / d1) * x + 0.75;
        }

        if (x < 2.5 / d1)
        {
            return n1 * (x -= 2.25 / d1) * x + 0.9375;
        }

        return n1 * (x -= 2.625 / d1) * x + 0.984375;
    }

    private Expression? GetExpression(FuncContext ctx)
    {
        Float1 x = ctx.GetValue(Value);
        return Easing.Value switch
        {
            EasingType.Linear => ctx.GetValue(Value),
            EasingType.InSine => ctx.Builder.Functions.GetInSine(x),
            EasingType.OutSine => ctx.Builder.Functions.GetOutSine(x),
            EasingType.InOutSine => ctx.Builder.Functions.GetInOutSine(x),
            EasingType.InQuad => ctx.Builder.Functions.GetInQuad(x),
            EasingType.OutQuad => ctx.Builder.Functions.GetOutQuad(x),
            EasingType.InOutQuad => ctx.Builder.Functions.GetInOutQuad(x),
            EasingType.InCubic => ctx.Builder.Functions.GetInCubic(x),
            EasingType.OutCubic => ctx.Builder.Functions.GetOutCubic(x),
            EasingType.InOutCubic => ctx.Builder.Functions.GetInOutCubic(x),
            EasingType.InQuart => ctx.Builder.Functions.GetInQuart(x),
            EasingType.OutQuart => ctx.Builder.Functions.GetOutQuart(x),
            EasingType.InOutQuart => ctx.Builder.Functions.GetInOutQuart(x),
            EasingType.InQuint => ctx.Builder.Functions.GetInQuint(x),
            EasingType.OutQuint => ctx.Builder.Functions.GetOutQuint(x),
            EasingType.InOutQuint => ctx.Builder.Functions.GetInOutQuint(x),
            EasingType.InExpo => ctx.Builder.Functions.GetInExpo(x),
            EasingType.OutExpo => ctx.Builder.Functions.GetOutExpo(x),
            EasingType.InOutExpo => ctx.Builder.Functions.GetInOutExpo(x),
            EasingType.InCirc => ctx.Builder.Functions.GetInCirc(x),
            EasingType.OutCirc => ctx.Builder.Functions.GetOutCirc(x),
            EasingType.InOutCirc => ctx.Builder.Functions.GetInOutCirc(x),
            EasingType.InBack => ctx.Builder.Functions.GetInBack(x),
            EasingType.OutBack => ctx.Builder.Functions.GetOutBack(x),
            EasingType.InOutBack => ctx.Builder.Functions.GetInOutBack(x),
            EasingType.InElastic => ctx.Builder.Functions.GetInElastic(x),
            EasingType.OutElastic => ctx.Builder.Functions.GetOutElastic(x),
            EasingType.InOutElastic => ctx.Builder.Functions.GetInOutElastic(x),
            EasingType.InBounce => ctx.Builder.Functions.GetInBounce(x),
            EasingType.OutBounce => ctx.Builder.Functions.GetOutBounce(x),
            EasingType.InOutBounce => ctx.Builder.Functions.GetInOutBounce(x),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public override Node CreateCopy()
    {
        return new EasingNode();
    }
}

public enum EasingType
{
    Linear,
    InSine,
    OutSine,
    InOutSine,
    InQuad,
    OutQuad,
    InOutQuad,
    InCubic,
    OutCubic,
    InOutCubic,
    InQuart,
    OutQuart,
    InOutQuart,
    InQuint,
    OutQuint,
    InOutQuint,
    InExpo,
    OutExpo,
    InOutExpo,
    InCirc,
    OutCirc,
    InOutCirc,
    InBack,
    OutBack,
    InOutBack,
    InElastic,
    OutElastic,
    InOutElastic,
    InBounce,
    OutBounce,
    InOutBounce
}
