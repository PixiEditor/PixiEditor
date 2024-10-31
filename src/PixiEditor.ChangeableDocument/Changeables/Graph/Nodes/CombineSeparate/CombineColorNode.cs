using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Shaders.Generation.Expressions;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

[NodeInfo("CombineColor")]
public class CombineColorNode : Node
{
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

        V1 = CreateFuncInput<Float1>("R", "R", 0d);
        V2 = CreateFuncInput<Float1>("G", "G", 0d);
        V3 = CreateFuncInput<Float1>("B", "B", 0d);
        A = CreateFuncInput<Float1>("A", "A", 0d);
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

        return ctx.NewHalf4(r, g, b, a); 
    }

    private Half4 GetHsv(FuncContext ctx)
    {
        var h = ctx.GetValue(V1);
        var s = ctx.GetValue(V2);
        var v = ctx.GetValue(V3);
        var a = ctx.GetValue(A);

        return ctx.HsvaToRgba(h, s, v, a);
    }
    
    private Half4 GetHsl(FuncContext ctx)
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
