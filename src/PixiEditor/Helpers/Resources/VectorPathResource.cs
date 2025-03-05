using Avalonia.Media;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.Helpers.Extensions;
using Color = Avalonia.Media.Color;

namespace PixiEditor.Helpers.Resources;

public class VectorPathResource
{
    public string SvgPath { get; set; }
    public StrokeCap StrokeLineCap { get; set; } = StrokeCap.Round;
    public StrokeJoin StrokeLineJoin { get; set; } = StrokeJoin.Round;
    public IBrush FillColor { get; set; } = new SolidColorBrush(Avalonia.Media.Colors.Transparent);
    public IBrush StrokeColor { get; set; } = new SolidColorBrush(Avalonia.Media.Colors.Black);
    public float StrokeWidth { get; set; } = 1;
    public PathFillType FillType { get; set; } = PathFillType.Winding;
    
    public PathVectorData ToVectorPathData()
    {
        VectorPath path = VectorPath.FromSvgPath(SvgPath);
        path.FillType = FillType;
        
        return new PathVectorData(path)
        {
            StrokeLineCap = StrokeLineCap,
            StrokeLineJoin = StrokeLineJoin,
            FillPaintable = FillColor.ToPaintable(),
            Stroke = StrokeColor.ToPaintable(),
            StrokeWidth = StrokeWidth
        };
    }
}
