using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Shaders.Generation.Expressions;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

[NodeInfo("CombineColor")]
public class CombineColorNode : Node
{
    public FuncOutputProperty<Half4, ShaderFuncContext> Color { get; }

    public InputProperty<CombineSeparateColorMode> Mode { get; }

    /// <summary>
    /// Represents either Red 'R' or Hue 'H' depending on <see cref="Mode"/>
    /// </summary>
    public FuncInputProperty<Float1, ShaderFuncContext> V1 { get; }

    /// <summary>
    /// Represents either Green 'G' or Saturation 'S' depending on <see cref="Mode"/>
    /// </summary>
    public FuncInputProperty<Float1, ShaderFuncContext> V2 { get; }

    /// <summary>
    /// Represents either Blue 'B', Value 'V' or Lightness 'L' depending on <see cref="Mode"/>
    /// </summary>
    public FuncInputProperty<Float1, ShaderFuncContext> V3 { get; }

    public FuncInputProperty<Float1, ShaderFuncContext> A { get; }

    public CombineColorNode()
    {
        Color = CreateFuncOutput<Half4, ShaderFuncContext>(nameof(Color), "COLOR", GetColor);
        Mode = CreateInput("Mode", "MODE", CombineSeparateColorMode.RGB);

        V1 = CreateFuncInput<Float1, ShaderFuncContext>("R", "R", 0d);
        V2 = CreateFuncInput<Float1, ShaderFuncContext>("G", "G", 0d);
        V3 = CreateFuncInput<Float1, ShaderFuncContext>("B", "B", 0d);
        A = CreateFuncInput<Float1, ShaderFuncContext>("A", "A", 0d);
    }

    private Half4 GetColor(ShaderFuncContext ctx) =>
        Mode.Value switch
        {
            CombineSeparateColorMode.RGB => GetRgb(ctx),
            CombineSeparateColorMode.HSV => GetHsv(ctx),
            CombineSeparateColorMode.HSL => GetHsl(ctx)
        };

    private Half4 GetRgb(ShaderFuncContext ctx)
    {
        var r = ctx.GetValue(V1);
        var g = ctx.GetValue(V2);
        var b = ctx.GetValue(V3);
        var a = ctx.GetValue(A);

        return ctx.NewHalf4(r, g, b, a); 
    }

    private Half4 GetHsv(ShaderFuncContext ctx)
    {
        var h = ctx.GetValue(V1);
        var s = ctx.GetValue(V2);
        var v = ctx.GetValue(V3);
        var a = ctx.GetValue(A);

        return ctx.HsvaToRgba(h, s, v, a);
    }
    
    private Half4 GetHsl(ShaderFuncContext ctx)
    {
        var h = ctx.GetValue(V1);
        var s = ctx.GetValue(V2);
        var l = ctx.GetValue(V3);
        var a = ctx.GetValue(A);

        return ctx.HslaToRgba(h, s, l, a);
    }

    protected override void OnExecute(RenderContext context)
    {
    }


    public override Node CreateCopy() => new CombineColorNode();
}
