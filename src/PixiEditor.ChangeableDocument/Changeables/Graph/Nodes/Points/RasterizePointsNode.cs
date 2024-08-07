using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Points;

[NodeInfo("RasterizePoints")]
public class RasterizePointsNode : Node
{
    private Paint _paint = new();

    public override string DisplayName { get; set; } = "RASTERIZE_POINTS";
    
    public OutputProperty<Surface> Image { get; }

    public InputProperty<PointList> Points { get; }

    public FuncInputProperty<Color> Color { get; }

    public RasterizePointsNode()
    {
        Image = CreateOutput<Surface>("Image", "IMAGE", null);
        Points = CreateInput("Points", "POINTS", PointList.Empty);
        Color = CreateFuncInput("Color", "COLOR", Colors.White);
    }

    protected override Surface? OnExecute(RenderingContext context)
    {
        var points = Points.Value;

        if (points.Count == 0)
            return null;

        var size = context.DocumentSize;
        var image = new Surface(size);

        var colorFunc = Color.Value;
        var funcContext = new FuncContext();
        foreach (var point in points)
        {
            funcContext.UpdateContext(point, context.DocumentSize);
            _paint.Color = colorFunc(funcContext);
            image.DrawingSurface.Canvas.DrawPixel((VecI)point.Multiply(size), _paint);
        }

        Image.Value = image;
        
        return image;
    }

    public override Node CreateCopy() => new RasterizePointsNode();
}
