using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

[NodeInfo("SeparateColor", "SEPARATE_COLOR_NODE", Category = "COLOR")]
public class SeparateColorNode : Node
{
    public FuncInputProperty<Half4> Color { get; }
    
    public InputProperty<CombineSeparateColorMode> Mode { get; }

    public FuncOutputProperty<Float1> R { get; }
    
    public FuncOutputProperty<Float1> G { get; }
    
    public FuncOutputProperty<Float1> B { get; }
    
    public FuncOutputProperty<Float1> A { get; }
    
    public SeparateColorNode()
    {
        R = CreateFuncOutput<Float1>(nameof(R), "R", ctx => GetColor(ctx).R);
        G = CreateFuncOutput<Float1>(nameof(G), "G", ctx => GetColor(ctx).G);
        B = CreateFuncOutput<Float1>(nameof(B), "B", ctx => GetColor(ctx).B);
        A = CreateFuncOutput<Float1>(nameof(A), "A", ctx => GetColor(ctx).A);
        Mode = CreateInput("Mode", "MODE", CombineSeparateColorMode.RGB);
        Color = CreateFuncInput<Half4>(nameof(Color), "COLOR", new Color());
    }

    protected override Texture? OnExecute(RenderingContext context)
    {
        return null;
    }
    
    private Half4 GetColor(FuncContext ctx) =>
        Mode.Value switch
        {
            CombineSeparateColorMode.RGB => GetRgba(ctx),
            CombineSeparateColorMode.HSV => GetHsva(ctx),
            CombineSeparateColorMode.HSL => GetHsla(ctx)
        };

    private Half4 GetRgba(FuncContext ctx) => ctx.Builder.GetOrNewAttachedHalf4(this.GetHashCode(), Color.GetHashCode(), () => Color.Value(ctx));

    private Half4 GetHsva(FuncContext ctx)
    {
        if (!ctx.HasContext && ctx.GetValue(Color) is Half4 constantColor)
        {
            var variable = new Half4(string.Empty);
            constantColor.ConstantValue.ToHsv(out float h, out float s, out float l);
            variable.ConstantValue = new Color((byte)(h * 255), (byte)(s * 255), (byte)(l * 255), constantColor.ConstantValue.A);
            
            return variable;
        }

        return ctx.Builder.GetOrNewAttachedHalf4(this.GetHashCode(), Color.GetHashCode(), RgbToHsvGetter);

        Expression RgbToHsvGetter() => ctx.Builder.Functions.GetRgbToHsv(ctx.GetValue(Color));
    }

    private Half4 GetHsla(FuncContext ctx)
    {
        if (!ctx.HasContext && ctx.GetValue(Color) is Half4 constantColor)
        {
            var variable = new Half4(string.Empty);
            constantColor.ConstantValue.ToHsl(out float h, out float s, out float l);
            variable.ConstantValue = new Color((byte)(h * 255), (byte)(s * 255), (byte)(l * 255), constantColor.ConstantValue.A);
            
            return variable;
        }

        return ctx.Builder.GetOrNewAttachedHalf4(this.GetHashCode(), Color.GetHashCode(), RgbToHslGetter);

        Expression RgbToHslGetter() => ctx.Builder.Functions.GetRgbToHsl(ctx.GetValue(Color));
    }

    public override Node CreateCopy() => new SeparateColorNode();
}
