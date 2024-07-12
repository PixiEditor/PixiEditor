using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace PixiEditor.AvaloniaUI.Views.Nodes;

public class ConnectionLine : Control
{
    public static readonly StyledProperty<SolidColorBrush> ColorProperty = AvaloniaProperty.Register<ConnectionLine, SolidColorBrush>("Color");
    public static readonly StyledProperty<double> ThicknessProperty = AvaloniaProperty.Register<ConnectionLine, double>("Thickness");
    public static readonly StyledProperty<Point> StartPointProperty = AvaloniaProperty.Register<ConnectionLine, Point>("StartPoint");
    public static readonly StyledProperty<Point> EndPointProperty = AvaloniaProperty.Register<ConnectionLine, Point>("EndPoint");

    public SolidColorBrush Color
    {
        get { return (SolidColorBrush)GetValue(ColorProperty); }
        set { SetValue(ColorProperty, value); }
    }

    public double Thickness
    {
        get { return (double)GetValue(ThicknessProperty); }
        set { SetValue(ThicknessProperty, value); }
    }

    public Point StartPoint
    {
        get { return (Point)GetValue(StartPointProperty); }
        set { SetValue(StartPointProperty, value); }
    }

    public Point EndPoint
    {
        get { return (Point)GetValue(EndPointProperty); }
        set { SetValue(EndPointProperty, value); }
    }
    
    static ConnectionLine()
    {
        AffectsRender<ConnectionLine>(ColorProperty, ThicknessProperty, StartPointProperty, EndPointProperty);
    }

    public override void Render(DrawingContext context)
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
        
        context.DrawGeometry(Color, new Pen(Color, Thickness), geometry);
    }
}
