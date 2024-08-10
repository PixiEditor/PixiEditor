using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

[NodeInfo("SeparateColor", "SEPARATE_COLOR_NODE")]
public class SeparateColorNode : Node
{
    public FuncOutputProperty<double> RH { get; }

    public FuncOutputProperty<double> GS { get; }

    public FuncOutputProperty<double> BVL { get; }

    public FuncOutputProperty<double> A { get; }

    public InputProperty<CombineSeparateColorMode> Mode { get; }

    public FuncInputProperty<Color> Color { get; }

    public SeparateColorNode()
    {
        // TODO: Mode based naming
        RH = CreateFuncOutput(nameof(RH), "RH", GetRHValue);
        GS = CreateFuncOutput(nameof(GS), "GS", GetGSValue);
        BVL = CreateFuncOutput(nameof(BVL), "BVL", GetBVLValue);
        A = CreateFuncOutput(nameof(A), "A", ctx => Color.Value(ctx).A / 255d);
        Mode = CreateInput("Mode", "MODE", CombineSeparateColorMode.RGB);
        Color = CreateFuncInput(nameof(Color), "COLOR", new Color());
    }

    private double GetRHValue(FuncContext ctx) => Mode.Value switch
    {
        CombineSeparateColorMode.RGB => Color.Value(ctx).R / 255d,
        CombineSeparateColorMode.HSV or CombineSeparateColorMode.HSL => GetHue(Color.Value(ctx))
    };

    private double GetHue(Color color)
    {
        color.ToHsv(out var hue, out _, out _);
        return hue / 360d;
    }

    private double GetGSValue(FuncContext ctx) => Mode.Value switch
    {
        CombineSeparateColorMode.RGB => Color.Value(ctx).G / 255d,
        CombineSeparateColorMode.HSV => GetHsvSaturation(Color.Value(ctx)),
        CombineSeparateColorMode.HSL => GetHslSaturation(Color.Value(ctx))
    };

    private double GetHsvSaturation(Color color)
    {
        color.ToHsv(out _, out var saturation, out _);
        return saturation;
    }

    private double GetHslSaturation(Color color)
    {
        color.ToHsl(out _, out var saturation, out _);
        return saturation;
    }

    private double GetBVLValue(FuncContext ctx) => Mode.Value switch
    {
        CombineSeparateColorMode.RGB => Color.Value(ctx).B / 255d,
        CombineSeparateColorMode.HSV => GetValue(Color.Value(ctx)),
        CombineSeparateColorMode.HSL => GetLightness(Color.Value(ctx))
    };

    private double GetValue(Color color)
    {
        color.ToHsv(out _, out _, out var value);
        return value;
    }
    
    private double GetLightness(Color color)
    {
        color.ToHsl(out _, out _, out var lightness);
        return lightness;
    }

    protected override Surface? OnExecute(RenderingContext context)
    {
        return null;
    }

    public override Node CreateCopy() => new SeparateColorNode();
}
