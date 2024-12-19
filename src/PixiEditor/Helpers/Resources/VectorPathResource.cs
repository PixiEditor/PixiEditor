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
    public Color FillColor { get; set; } = Avalonia.Media.Colors.Transparent;
    public Color StrokeColor { get; set; } = Avalonia.Media.Colors.Black;
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
            FillColor = FillColor.ToColor(),
            StrokeColor = StrokeColor.ToColor(),
            StrokeWidth = StrokeWidth
        };
    }
}
