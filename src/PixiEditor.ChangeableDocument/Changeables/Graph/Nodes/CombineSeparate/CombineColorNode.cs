using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Shaders.Generation.Expressions;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

[NodeInfo(UniqueName)]
public class CombineColorNode : Node
{
    public const string UniqueName = "CombineColor";
    public const string ModePropertyName = "Mode";
    public const string V1PropertyName = "R";
    public const string V2PropertyName = "G";
    public const string V3PropertyName = "B";
    public const string APropertyName = "A";

    public FuncOutputProperty<Half4> Color { get; }

    public InputProperty<CombineSeparateColorMode> Mode { get; }

    /// <summary>
    /// Represents either Red 'R' or Hue 'H' depending on <see cref="Mode"/>
    /// </summary>
    public FuncInputProperty<Float1> V1 { get; }

    /// <summary>
    /// Represents either Green 'G' or Saturation 'S' depending on <see cref="Mode"/>
    /// </summary>
    public FuncInputProperty<Float1> V2 { get; }

    /// <summary>
    /// Represents either Blue 'B', Value 'V' or Lightness 'L' depending on <see cref="Mode"/>
    /// </summary>
    public FuncInputProperty<Float1> V3 { get; }

    public FuncInputProperty<Float1> A { get; }

    public CombineColorNode()
    {
        Color = CreateFuncOutput(nameof(Color), "COLOR", GetColor);
        Mode = CreateInput("Mode", "MODE", CombineSeparateColorMode.RGB);

        V1 = CreateFuncInput<Float1>(V1PropertyName, "R", 0d);
        V2 = CreateFuncInput<Float1>(V2PropertyName, "G", 0d);
        V3 = CreateFuncInput<Float1>(V3PropertyName, "B", 0d);
        A = CreateFuncInput<Float1>(APropertyName, "A", 0d);
    }

    private Half4 GetColor(FuncContext ctx) =>
        Mode.Value switch
        {
            CombineSeparateColorMode.RGB => GetRgb(ctx),
            CombineSeparateColorMode.HSV => GetHsv(ctx),
            CombineSeparateColorMode.HSL => GetHsl(ctx)
        };

    private Half4 GetRgb(FuncContext ctx)
    {
        var r = ctx.GetValue(V1);
        var g = ctx.GetValue(V2);
        var b = ctx.GetValue(V3);
        var a = ctx.GetValue(A);

        if (ctx.HasContext)
        {
            AdjustConstValue(ctx, r, 255f);
            AdjustConstValue(ctx, g, 255f);
            AdjustConstValue(ctx, b, 255f);
            AdjustConstValue(ctx, a, 255f);
        }

        return ctx.NewHalf4(r, g, b, a);
    }

    private Half4 GetHsv(FuncContext ctx)
    {
        var h = ctx.GetValue(V1);
        var s = ctx.GetValue(V2);
        var v = ctx.GetValue(V3);
        var a = ctx.GetValue(A);

        if (ctx.HasContext)
        {
            AdjustConstValue(ctx, h, 360f);
            AdjustConstValue(ctx, s, 100f);
            AdjustConstValue(ctx, v, 100f);
            AdjustConstValue(ctx, a, 255f);
        }

        return ctx.HsvaToRgba(h, s, v, a);
    }

    private Half4 GetHsl(FuncContext ctx)
    {
        var h = ctx.GetValue(V1);
        var s = ctx.GetValue(V2);
        var l = ctx.GetValue(V3);
        var a = ctx.GetValue(A);
        if (ctx.HasContext)
        {
            AdjustConstValue(ctx, h, 360f);
            AdjustConstValue(ctx, s, 100f);
            AdjustConstValue(ctx, l, 100f);
            AdjustConstValue(ctx, a, 255f);
        }

        return ctx.HslaToRgba(h, s, l, a);
    }

    private static void AdjustConstValue(FuncContext ctx, Float1 uniform, float adjustBy)
    {
        var uniformVar = ctx.Builder.Uniforms.FirstOrDefault(x => x.Key == uniform.VariableName);
        ctx.Builder.Uniforms.Remove(uniform.VariableName);

        string uniformName = $"float_{ctx.Builder.GetUniqueNameNumber()}";
        ctx.Builder.AddUniform(uniformName, uniformVar.Value.FloatValue / adjustBy);
    }

    protected override void OnExecute(RenderContext context)
    {
    }


    public override Node CreateCopy() => new CombineColorNode();
}
