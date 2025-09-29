using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Drawie.Numerics;
using PixiEditor.Helpers.Converters;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.Views.Nodes;

public class ConnectionRenderer : Control
{
    public static readonly StyledProperty<SocketsInfo> SocketsInfoProperty =
        AvaloniaProperty.Register<ConnectionRenderer, SocketsInfo>(
            nameof(SocketsInfo));

    public SocketsInfo SocketsInfo
    {
        get => GetValue(SocketsInfoProperty);
        set => SetValue(SocketsInfoProperty, value);
    }

    internal static readonly StyledProperty<ObservableCollection<NodeConnectionViewModel>> ConnectionsProperty =
        AvaloniaProperty.Register<ConnectionRenderer, ObservableCollection<NodeConnectionViewModel>>(
            nameof(Connections));

    internal ObservableCollection<NodeConnectionViewModel> Connections
    {
        get => GetValue(ConnectionsProperty);
        set => SetValue(ConnectionsProperty, value);
    }

    private double thickness = 2;
    public static readonly StyledProperty<TransformGroup> ContentTransformProperty = AvaloniaProperty.Register<ConnectionRenderer, TransformGroup>("ContentTransform");

    public override void Render(DrawingContext context)
    {
        if (SocketsInfo == null || Connections == null)
        {
            return;
        }

        foreach (var connection in Connections)
        {
            var inputSocket =
                SocketsInfo.Sockets.TryGetValue($"i:{connection.InputNode.Id}.{connection.InputProperty.PropertyName}", out var socket)
                    ? socket
                    : null;
            var outputSocket =
                SocketsInfo.Sockets.TryGetValue($"o:{connection.OutputNode.Id}.{connection.OutputProperty.PropertyName}", out socket)
                    ? socket
                    : null;

            if (inputSocket == null || outputSocket == null || !inputSocket.IsVisible || !outputSocket.IsVisible)
            {
                continue;
            }

            Point startPoint = SocketsInfo.GetSocketPosition(inputSocket);
            Point endPoint = SocketsInfo.GetSocketPosition(outputSocket);

            LinearGradientBrush brush = new LinearGradientBrush
            {
                GradientStops = new GradientStops
                {
                    new GradientStop { Offset = 0, Color = Color.FromRgb(85, 85, 85) },
                    new GradientStop { Offset = 0.05, Color = SocketColorConverter.SocketToColor(connection.InputProperty.SocketBrush) },
                    new GradientStop { Offset = 0.95, Color = SocketColorConverter.SocketToColor(connection.OutputProperty.SocketBrush) },
                    new GradientStop { Offset = 1, Color = Color.FromRgb(85, 85, 85) }
                }
            };

            Pen pen = new() { LineCap = PenLineCap.Round };

            var p1 = new Point(startPoint.X, startPoint.Y);
            var p2 = new Point(endPoint.X, endPoint.Y);

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

            brush.StartPoint = new RelativePoint(new Point(0, 0), RelativeUnit.Relative);
            brush.EndPoint = new RelativePoint(new Point(1, 0), RelativeUnit.Relative);

            /*if (startPoint.X < endPoint.X)
            {
                (brush.StartPoint, brush.EndPoint) = (brush.EndPoint, brush.StartPoint);
            }*/

            pen.Brush = brush;
            pen.Thickness = thickness;

            context.DrawGeometry(brush, pen, geometry);
        }
    }
}
