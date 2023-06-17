using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.Views.UserControls.Guides;

namespace PixiEditor.Models.DataHolders.Guides;

internal class RectangleGuide : Guide
{
    private GrabbedPoint? grabbedPoint;

    private double left;
    private double top;
    private double width;
    private double height;
    private Color color;

    public double Left
    {
        get => left;
        set
        {
            if (SetProperty(ref left, value))
            {
                InvalidateVisual();
            }
        }
    }

    public double Top
    {
        get => top;
        set
        {
            if (SetProperty(ref top, value))
            {
                InvalidateVisual();
            }
        }
    }

    public double Width
    {
        get => width;
        set
        {
            if (SetProperty(ref width, value))
            {
                InvalidateVisual();
            }
        }
    }

    public double Height
    {
        get => height;
        set
        {
            if (SetProperty(ref height, value))
            {
                InvalidateVisual();
            }
        }
    }

    public Color Color
    {
        get => color;
        set
        {
            if (SetProperty(ref color, value))
            {
                InvalidateVisual();
            }
        }
    }

    public override Control SettingsControl { get; }

    public override string TypeNameKey => "RECTANGLE_GUIDE";

    public RectangleGuide(DocumentViewModel document) : base(document)
    {
        Color = Colors.CadetBlue;
        SettingsControl = new RectangleGuideSettings(this);
    }

    public override void Draw(DrawingContext context, GuideRenderer renderer)
    {
        bool skipDraw = false;

        var mod = IsEditing ? 3 : (ShowExtended ? 2 : 1);

        var pen = new Pen(new SolidColorBrush(color), renderer.ScreenUnit * 1.5d * mod);
        context.DrawRectangle(null, pen, new(Left, Top, Width, Height));
    }

    protected override void RendererAttached(GuideRenderer renderer)
    {
        renderer.MouseEnter += Renderer_MouseEnter;
        renderer.MouseLeave += Renderer_MouseLeave;

        renderer.MouseLeftButtonDown += Renderer_MouseLeftButtonDown;
        renderer.MouseMove += Renderer_MouseMove;
        renderer.MouseLeftButtonUp += Renderer_MouseLeftButtonUp;
    }

    private void Renderer_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (!IsEditing)
        {
            return;
        }

        var renderer = (GuideRenderer)sender;
        var closestPoint = GetPoint(e.GetPosition(renderer), renderer.ScreenUnit);

        renderer.Cursor = closestPoint switch
        {
            GrabbedPoint.TopLeft or GrabbedPoint.BottomRight => Cursors.SizeNWSE,
            GrabbedPoint.TopRight or GrabbedPoint.BottomLeft => Cursors.SizeNESW,
            _ => null
        };
    }

    private void Renderer_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        var renderer = (GuideRenderer)sender;
        renderer.Cursor = null;
    }

    private void Renderer_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (!IsEditing)
        {
            return;
        }

        e.Handled = true;
        var renderer = (GuideRenderer)sender;
        Mouse.Capture(renderer);
        grabbedPoint = GetPoint(e.GetPosition(renderer), renderer.ScreenUnit);
    }

    private void Renderer_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (!IsEditing)
        {
            return;
        }

        e.Handled = true;
        var renderer = (GuideRenderer)sender;
        var mousePos = e.GetPosition(renderer);

        if (grabbedPoint == null)
        {
            var closestPoint = GetPoint(mousePos, renderer.ScreenUnit);

            renderer.Cursor = closestPoint switch
            {
                GrabbedPoint.TopLeft or GrabbedPoint.BottomRight => Cursors.SizeNWSE,
                GrabbedPoint.TopRight or GrabbedPoint.BottomLeft => Cursors.SizeNESW,
                _ => null
            };
            return;
        }

        var size = Document.SizeBindable;

        var newLeft = Left;
        var newTop = Top;
        var newWidth = Width;
        var newHeight = Height;

        GetNewDimensions(mousePos, ref newLeft, ref newTop, ref newWidth, ref newHeight);
        if (EnsureSafeDimensions(ref newLeft, ref newTop, ref newWidth, ref newHeight))
        {
            return;
        }

        Left = newLeft;
        Top = newTop;
        Width = newWidth;
        Height = newHeight;
    }

    private void GetNewDimensions(Point mousePos, ref double newLeft, ref double newTop, ref double newWidth, ref double newHeight)
    {
        switch (grabbedPoint)
        {
            case GrabbedPoint.TopLeft:
                newWidth = RoundMod(Width + Left - mousePos.X);
                newHeight = RoundMod(Height + Top - mousePos.Y);
                newLeft = RoundMod(mousePos.X);
                newTop = RoundMod(mousePos.Y);
                break;
            case GrabbedPoint.TopRight:
                newWidth = RoundMod(mousePos.X - Left);
                newHeight = RoundMod(Height + Top - mousePos.Y);
                newTop = RoundMod(mousePos.Y);
                break;
            case GrabbedPoint.BottomRight:
                newWidth = RoundMod(mousePos.X - Left);
                newHeight = RoundMod(mousePos.Y - Top);
                break;
            case GrabbedPoint.BottomLeft:
                newWidth = RoundMod(Width + Left - mousePos.X);
                newLeft = RoundMod(mousePos.X);
                newHeight = RoundMod(mousePos.Y - Top);
                break;
        }
    }

    private bool EnsureSafeDimensions(ref double newLeft, ref double newTop, ref double newWidth, ref double newHeight)
    {
        if (newWidth < 0 && newHeight < 0)
        {
            newWidth = 0;
            newHeight = 0;

            grabbedPoint = grabbedPoint switch
            {
                GrabbedPoint.TopLeft => GrabbedPoint.BottomRight,
                GrabbedPoint.BottomRight => GrabbedPoint.TopLeft,
                GrabbedPoint.TopRight => GrabbedPoint.BottomLeft,
                GrabbedPoint.BottomLeft => GrabbedPoint.TopRight
            };

            return true;
        }

        if (newWidth < 0)
        {
            newWidth = 0;

            grabbedPoint = grabbedPoint switch
            {
                GrabbedPoint.TopLeft => GrabbedPoint.TopRight,
                GrabbedPoint.BottomLeft => GrabbedPoint.BottomRight,
                GrabbedPoint.TopRight => GrabbedPoint.TopLeft,
                GrabbedPoint.BottomRight => GrabbedPoint.BottomLeft
            };

            return true;
        }

        if (newHeight < 0)
        {
            newHeight = 0;

            grabbedPoint = grabbedPoint switch
            {
                GrabbedPoint.TopLeft => GrabbedPoint.BottomLeft,
                GrabbedPoint.TopRight => GrabbedPoint.BottomRight,
                GrabbedPoint.BottomLeft => GrabbedPoint.TopLeft,
                GrabbedPoint.BottomRight => GrabbedPoint.TopRight
            };

            return true;
        }

        return false;
    }

    private void Renderer_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (!IsEditing || grabbedPoint == null)
        {
            return;
        }

        grabbedPoint = null;
        var renderer = (GuideRenderer)sender;
        renderer.Cursor = null;
        Mouse.Capture(null);
    }

    private GrabbedPoint? GetPoint(Point mouse, double screenUnit)
    {
        var size = Document.SizeBindable;

        var topLeft = new Point(Left, Top);
        var topRight = new Point(Left + Width, Top);
        var bottomRight = new Point(Left + Width, Top + Height);
        var bottomLeft = new Point(Left, Top + Height);

        double value = double.PositiveInfinity;
        int index = -1;
        var points = new Point[] { topLeft, topRight, bottomRight, bottomLeft };

        for (int i = 0; i < points.Length; i++)
        {
            var length = (points[i] - mouse).Length;
            if (value > length)
            {
                value = length;
                index = i;
            }
        }

        if (value > screenUnit * 20)
        {
            return null;
        }

        return (GrabbedPoint)index;
    }

    private double RoundMod(double value)
    {
        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
        {
            return Math.Round(value);
        }
        else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
        {
            return value;
        }

        return Math.Round(value * 2, MidpointRounding.AwayFromZero) / 2;
    }

    enum GrabbedPoint
    {
        TopLeft,
        TopRight,
        BottomRight,
        BottomLeft
    }
}
