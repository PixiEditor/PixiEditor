using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

[NodeInfo(UniqueName)]
public class SeparateColorNode : Node
{
    public const string UniqueName = "SeparateColor";
    public const string ColorPropertyName = "Color";
    public const string V1PropertyName = "R";
    public const string V2PropertyName = "G";
    public const string V3PropertyName = "B";
    public const string APropertyName = "A";
    public const string ModePropertyName = "Mode";
    public const string NormalizedValuesPropertyName = "NormalizedValues";

    private readonly NodeVariableAttachments contextVariables = new();

    public FuncInputProperty<Half4> Color { get; }

    public InputProperty<CombineSeparateColorMode> Mode { get; }
    public InputProperty<bool> NormalizedValues { get; }

    /// <summary>
    /// Represents either Red 'R' or Hue 'H' depending on <see cref="Mode"/>
    /// </summary>
    public FuncOutputProperty<Float1> V1 { get; }

    /// <summary>
    /// Represents either Green 'G' or Saturation 'S' depending on <see cref="Mode"/>
    /// </summary>
    public FuncOutputProperty<Float1> V2 { get; }

    /// <summary>
    /// Represents either Blue 'B', Value 'V' or Lightness 'L' depending on <see cref="Mode"/>
    /// </summary>
    public FuncOutputProperty<Float1> V3 { get; }

    public FuncOutputProperty<Float1> A { get; }


    public SeparateColorNode()
    {
        V1 = CreateFuncOutput<Float1>(V1PropertyName, "R", ctx => GetColor(ctx).R);
        V2 = CreateFuncOutput<Float1>(V2PropertyName, "G", ctx => GetColor(ctx).G);
        V3 = CreateFuncOutput<Float1>(V3PropertyName, "B", ctx => GetColor(ctx).B);
        A = CreateFuncOutput<Float1>(APropertyName, "A", ctx => GetColor(ctx).A);
        Mode = CreateInput(ModePropertyName, "MODE", CombineSeparateColorMode.RGB);
        Color = CreateFuncInput<Half4>(ColorPropertyName, "COLOR", new Half4(Vec4D.Zero));
        NormalizedValues = CreateInput<bool>(NormalizedValuesPropertyName, "NORMALIZED_COLOR_VALUES", true);
    }

    protected override void OnExecute(RenderContext context)
    {
    }

    private Half4 GetColor(FuncContext ctx) =>
        Mode.Value switch
        {
            CombineSeparateColorMode.RGB => GetRgba(ctx),
            CombineSeparateColorMode.HSV => GetHsva(ctx),
            CombineSeparateColorMode.HSL => GetHsla(ctx),
            _ => GetRgba(ctx)
        };

    private Half4 GetRgba(FuncContext ctx)
    {
        return ctx.HasContext
            ? contextVariables.GetOrAttachNew(ctx, Color, () => AdjustForRgbaRange(ctx.GetValue(Color), ctx))
            : AdjustForRgbaRange(ctx.GetValue(Color), ctx);
    }

    private Half4 GetHsva(FuncContext ctx) =>
        ctx.HasContext
            ? contextVariables.GetOrAttachNew(ctx, Color, () => ctx.RgbaToHsva(AdjustForHsvRange(ctx.GetValue(Color), ctx)))
            : AdjustForRgbaRange(ctx.RgbaToHsva(AdjustForHsvRange(ctx.GetValue(Color), ctx)), ctx);

    private Half4 GetHsla(FuncContext ctx) =>
        ctx.HasContext
            ? contextVariables.GetOrAttachNew(ctx, Color, () => ctx.RgbaToHsla(AdjustForHslRange(ctx.GetValue(Color), ctx)))
            : AdjustForRgbaRange(ctx.RgbaToHsla(AdjustForHslRange(ctx.GetValue(Color), ctx)), ctx);

    private Half4 AdjustForRgbaRange(Half4 color, FuncContext ctx)
    {
        if (!NormalizedValues.Value)
        {
            if (!ctx.HasContext)
            {
                Half4 adjustedColor = new Half4(new Vec4D(color.R.GetConstant() is double r ? r * 255.0 : 0,
                    color.G.GetConstant() is double g ? g * 255.0 : 0,
                    color.B.GetConstant() is double b ? b * 255.0 : 0,
                    color.A.GetConstant() is double a ? a * 255.0 : 0));
                return adjustedColor;
            }

            return ctx.NewHalf4(
                ShaderMath.Multiply(color.R, new Float1("") { ConstantValue = 255.0 }),
                ShaderMath.Multiply(color.G, new Float1("") { ConstantValue = 255.0 }),
                ShaderMath.Multiply(color.B, new Float1("") { ConstantValue = 255.0 }),
                ShaderMath.Multiply(color.A, new Float1("") { ConstantValue = 255.0 })
            );
        }

        return color;

    }

    private Half4 AdjustForHsvRange(Half4 color, FuncContext ctx)
    {
        if (!NormalizedValues.Value)
        {
            if (!ctx.HasContext)
            {
                Half4 adjustedColor = new Half4(new Vec4D(color.R.GetConstant() is double r ? r * 360.0 : 0,
                    color.G.GetConstant() is double g ? g * 100.0 : 0,
                    color.B.GetConstant() is double b ? b * 100.0 : 0,
                    color.A.GetConstant() is double a ? a * 255.0 : 0));
                return adjustedColor;
            }

            return ctx.NewHalf4(
                ShaderMath.Multiply(color.R, new Float1("") { ConstantValue = 360.0 }),
                ShaderMath.Multiply(color.G, new Float1("") { ConstantValue = 100.0 }),
                ShaderMath.Multiply(color.B, new Float1("") { ConstantValue = 100.0 }),
                ShaderMath.Multiply(color.A, new Float1("") { ConstantValue = 255.0 })
            );
        }

        return color;
    }

    private Half4 AdjustForHslRange(Half4 color, FuncContext ctx)
    {
        if (!NormalizedValues.Value)
        {
            if (!ctx.HasContext)
            {
                Half4 adjustedColor = new Half4(new Vec4D(color.R.GetConstant() is double r ? r * 360.0 : 0,
                    color.G.GetConstant() is double g ? g * 100.0 : 0,
                    color.B.GetConstant() is double b ? b * 100.0 : 0,
                    color.A.GetConstant() is double a ? a * 255.0 : 0));

                return adjustedColor;
            }

            return ctx.NewHalf4(
                ShaderMath.Multiply(color.R, new Float1("") { ConstantValue = 360.0 }),
                ShaderMath.Multiply(color.G, new Float1("") { ConstantValue = 100.0 }),
                ShaderMath.Multiply(color.B, new Float1("") { ConstantValue = 100.0 }),
                ShaderMath.Multiply(color.A, new Float1("") { ConstantValue = 255.0 })
            );
        }

        return color;
    }

    public override Node CreateCopy() => new SeparateColorNode();
}
