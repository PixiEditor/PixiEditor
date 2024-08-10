using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

[NodeInfo("CombineColor", "COMBINE_COLOR_NODE")]
public class CombineColorNode : Node
{
    public FuncOutputProperty<Color> Color { get; }

    public InputProperty<CombineSeparateColorMode> Mode { get; }

    public FuncInputProperty<double> RH { get; }

    public FuncInputProperty<double> GS { get; }

    public FuncInputProperty<double> BVL { get; }

    public FuncInputProperty<double> A { get; }

    public CombineColorNode()
    {
        Color = CreateFuncOutput(nameof(Color), "COLOR", GetColor);
        Mode = CreateInput("Mode", "MODE", CombineSeparateColorMode.RGB);

        // TODO: Mode based naming
        RH = CreateFuncInput("R", "RH", 0d);
        GS = CreateFuncInput("G", "GS", 0d);
        BVL = CreateFuncInput("B", "BVL", 0d);
        A = CreateFuncInput("A", "A", 0d);
    }

    private Color GetColor(FuncContext ctx)
    {
        var mode = Mode.Value;
        
        var rh = RH.Value(ctx);
        var gs = GS.Value(ctx);
        var bvl = BVL.Value(ctx);
        var a = (byte)(A.Value(ctx) * 255);

        return mode switch
        {
            CombineSeparateColorMode.RGB => new Color((byte)(rh * 255), (byte)(gs * 255), (byte)(bvl * 255), a),
            CombineSeparateColorMode.HSV => DrawingApi.Core.ColorsImpl.Color.FromHsv((float)(rh * 360d), (float)gs, (float)bvl, a),
            CombineSeparateColorMode.HSL => DrawingApi.Core.ColorsImpl.Color.FromHsl((float)(rh * 360d), (float)gs, (float)bvl, a)
        };
    }


    protected override Surface? OnExecute(RenderingContext context)
    {
        return null;
    }


    public override Node CreateCopy() => new CombineColorNode();
}
