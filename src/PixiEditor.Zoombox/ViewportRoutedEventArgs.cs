using ChunkyImageLib.DataHolders;
using System.Windows;

namespace PixiEditor.Zoombox;

public class ViewportRoutedEventArgs : RoutedEventArgs
{
    public ViewportRoutedEventArgs(RoutedEvent e, Vector2d center, Vector2d size, Vector2d realSize, double angle) : base(e)
    {
        Center = center;
        Size = size;
        RealSize = realSize;
        Angle = angle;
    }

    public Vector2d Center { get; }
    public Vector2d Size { get; }
    public Vector2d RealSize { get; }
    public double Angle { get; }
}
