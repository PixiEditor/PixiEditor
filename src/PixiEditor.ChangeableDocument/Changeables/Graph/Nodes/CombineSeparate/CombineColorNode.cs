using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

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
    public const string NormalizedValuesPropertyName = "NormalizedValues";

    public FuncOutputProperty<Half4> Color { get; }

    public InputProperty<CombineSeparateColorMode> Mode { get; }
    public InputProperty<bool> NormalizedValues { get; }

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
        NormalizedValues = CreateInput(NormalizedValuesPropertyName, "NORMALIZED_COLOR_VALUES", true);
    }

    private Half4 GetColor(FuncContext ctx) =>
        Mode.Value switch
        {
            CombineSeparateColorMode.RGB => GetRgb(ctx),
            CombineSeparateColorMode.HSV => GetHsv(ctx),
            CombineSeparateColorMode.HSL => GetHsl(ctx)
        };

    internal override void DeserializeAdditionalDataInternal(IReadOnlyDocument target,
        IReadOnlyDictionary<string, object> data, List<IChangeInfo> infos)
    {
        if (data.TryGetValue("usesLegacy255Range", out var usesLegacy255RangeObj) &&
            usesLegacy255RangeObj is bool usesLegacy255RangeBool)
        {
            NormalizedValues.NonOverridenValue = !usesLegacy255RangeBool;
            infos.Add(new PropertyValueUpdated_ChangeInfo(Id, NormalizedValuesPropertyName, !usesLegacy255RangeBool));
        }
    }

    private Half4 GetRgb(FuncContext ctx)
    {
        var r = ctx.GetValue(V1);
        var g = ctx.GetValue(V2);
        var b = ctx.GetValue(V3);
        var a = ctx.GetValue(A);

        if (!NormalizedValues.Value)
        {
            if (ctx.HasContext)
            {
                r = AdjustConstValue(ctx, r, 255f);
                g = AdjustConstValue(ctx, g, 255f);
                b = AdjustConstValue(ctx, b, 255f);
                a = AdjustConstValue(ctx, a, 255f);
            }
            else
            {
                if (r.GetConstant() is double rConst)
                {
                    r = new Float1("") { ConstantValue = rConst / 255.0 };
                }

                if (g.GetConstant() is double gConst)
                {
                    g = new Float1("") { ConstantValue = gConst / 255.0 };
                }

                if (b.GetConstant() is double bConst)
                {
                    b = new Float1("") { ConstantValue = bConst / 255.0 };
                }

                if (a.GetConstant() is double aConst)
                {
                    a = new Float1("") { ConstantValue = aConst / 255.0 };
                }
            }
        }

        return ctx.NewHalf4(r, g, b, a);
    }

    private Half4 GetHsv(FuncContext ctx)
    {
        var h = ctx.GetValue(V1);
        var s = ctx.GetValue(V2);
        var v = ctx.GetValue(V3);
        var a = ctx.GetValue(A);

        if (!NormalizedValues.Value)
        {
            if (ctx.HasContext)
            {
                h = AdjustConstValue(ctx, h, 360f);
                s = AdjustConstValue(ctx, s, 100f);
                v = AdjustConstValue(ctx, v, 100f);
                a = AdjustConstValue(ctx, a, 255f);
            }
            else
            {
                if (h.GetConstant() is double hConst)
                {
                    h = new Float1("") { ConstantValue = hConst / 360.0 };
                }

                if (s.GetConstant() is double sConst)
                {
                    s = new Float1("") { ConstantValue = sConst / 100.0 };
                }

                if (v.GetConstant() is double vConst)
                {
                    v = new Float1("") { ConstantValue = vConst / 100.0 };
                }

                if (a.GetConstant() is double aConst)
                {
                    a = new Float1("") { ConstantValue = aConst / 255.0 };
                }
            }
        }

        return ctx.HsvaToRgba(h, s, v, a);
    }

    private Half4 GetHsl(FuncContext ctx)
    {
        var h = ctx.GetValue(V1);
        var s = ctx.GetValue(V2);
        var l = ctx.GetValue(V3);
        var a = ctx.GetValue(A);

        if (!NormalizedValues.Value)
        {
            if (ctx.HasContext)
            {
                h = AdjustConstValue(ctx, h, 360f);
                s = AdjustConstValue(ctx, s, 100f);
                l = AdjustConstValue(ctx, l, 100f);
                a = AdjustConstValue(ctx, a, 255f);
            }
            else
            {
                if (h.GetConstant() is double hConst)
                {
                    h = new Float1("") { ConstantValue = hConst / 360.0 };
                }

                if (s.GetConstant() is double sConst)
                {
                    s = new Float1("") { ConstantValue = sConst / 100.0 };
                }

                if (l.GetConstant() is double lConst)
                {
                    l = new Float1("") { ConstantValue = lConst / 100.0 };
                }

                if (a.GetConstant() is double aConst)
                {
                    a = new Float1("") { ConstantValue = aConst / 255.0 };
                }
            }
        }

        return ctx.HslaToRgba(h, s, l, a);
    }

    private static Float1 AdjustConstValue(FuncContext ctx, Float1 uniform, float adjustBy)
    {
        var uniformVar = ctx.Builder.Uniforms.FirstOrDefault(x => x.Key == uniform.VariableName);
        if (!string.IsNullOrEmpty(uniformVar.Key))
        {
            ctx.Builder.Uniforms.Remove(uniform.VariableName);
            ctx.Builder.AddUniform(uniform.VariableName, uniformVar.Value.FloatValue / adjustBy);
            return uniform;
        }

        return ctx.NewFloat1(new Expression($"({uniform.VariableName} / {adjustBy})"));
    }

    protected override void OnExecute(RenderContext context)
    {
    }


    public override Node CreateCopy() => new CombineColorNode();
}
