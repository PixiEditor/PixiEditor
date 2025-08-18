using Avalonia;
using Avalonia.Media;
using Drawie.Numerics;

namespace PixiEditor.Views.Nodes;

public class Connection
{
    public VecD StartPoint { get; set; }
    public VecD EndPoint { get; set; }
    public LinearGradientBrush LineBrush { get; set; }
    public double Thickness { get; set; }

    private Pen pen = new() { LineCap = PenLineCap.Round };

    public void Render(DrawingContext context)
    {
        var p1 = new Point(StartPoint.X, StartPoint.Y);
        var p2 = new Point(EndPoint.X, EndPoint.Y);

        // curved line
        var controlPoint = new Point((p1.X + p2.X) / 2, p1.Y);
        var controlPoint2 = new Point((p1.X + p2.X) / 2, p2.Y);

        if (p1.X < p2.X)
        {
            p1 = new Point(p1.X - 5, p1.Y);
            p2 = new Point(p2.X + 5, p2.Y);

            controlPoint2 = new Point(p2.X, (p1.Y + p2.Y) / 2);
            controlPoint = new Point(p1.X, (p1.Y + p2.Y) / 2);
        }

        var geometry = new StreamGeometry();
        using var ctx = geometry.Open();
        ctx.BeginFigure(p1, false);
        ctx.CubicBezierTo(controlPoint, controlPoint2, p2);

        LineBrush.StartPoint = new RelativePoint(p1.X, p1.Y, RelativeUnit.Absolute);
        LineBrush.EndPoint = new RelativePoint(p2.X, p2.Y, RelativeUnit.Absolute);

        pen.Brush = LineBrush;
        pen.Thickness = Thickness;

        context.DrawGeometry(LineBrush, pen, geometry);
    }
}
