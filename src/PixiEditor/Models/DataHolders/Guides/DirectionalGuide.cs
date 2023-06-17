using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.Views.UserControls.Guides;

namespace PixiEditor.Models.DataHolders.Guides;

internal class DirectionalGuide : Guide
{
    private double offset;
    private bool isDragging;
    private Color color;
    private Direction direction;

    private bool IsVertical => direction == Direction.Vertical;

    public override Control SettingsControl { get; }

    public override string TypeNameKey => IsVertical ? "VERTICAL_GUIDE" : "HORIZONTAL_GUIDE";

    public override string IconPath
    {
        get
        {
            var name = IsVertical ? "VerticalGuide" : "HorizontalGuide";
            return $"/Images/Guides/{name}.png";
        }
    }

    public double Offset
    {
        get => offset;
        set
        {
            if (SetProperty(ref offset, value))
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

    public string OffsetName => IsVertical ? "X" : "Y";

    public DirectionalGuide(DocumentViewModel document, Direction direction) : base(document)
    {
        Color = Colors.CadetBlue;
        this.direction = direction;
        SettingsControl = new DirectionalGuideSettings(this);
    }

    public override void Draw(DrawingContext context, GuideRenderer renderer)
    {
        var mod = IsEditing ? 4.5 : (ShowExtended ? 3 : 1.5);
        var pen = new Pen(new SolidColorBrush(Color), renderer.ScreenUnit * mod);

        var pointA = IsVertical ? new Point(Offset, 0) : new Point(0, Offset);
        var pointB = IsVertical ? new Point(Offset, Document.SizeBindable.Y) : new Point(Document.SizeBindable.X, Offset);

        context.DrawLine(pen, pointA, pointB);
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
        if (IsEditing)
        {
            var renderer = (GuideRenderer)sender;
            renderer.Cursor = direction == Direction.Vertical ? Cursors.ScrollWE : Cursors.ScrollNS;
        }
    }

    private void Renderer_MouseLeave(object sender, MouseEventArgs e)
    {
        var renderer = (GuideRenderer)sender;
        renderer.Cursor = null;
    }

    private void Renderer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!IsEditing)
        {
            return;
        }

        var renderer = (GuideRenderer)sender;
        e.Handled = true;
        isDragging = true;
        Mouse.Capture(renderer);
    }

    private void Renderer_MouseMove(object sender, MouseEventArgs e)
    {
        if (!isDragging)
        {
            return;
        }

        e.Handled = true;

        var renderer = (GuideRenderer)sender;
        var position = e.GetPosition(renderer);

        var offset = IsVertical ? position.X : position.Y;
        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
        {
            Offset = Math.Round(offset, MidpointRounding.AwayFromZero);
        }
        else
        {
            Offset = Math.Round(offset * 2, MidpointRounding.AwayFromZero) / 2;
        }
    }

    private void Renderer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!isDragging)
        {
            return;
        }

        Mouse.Capture(null);
        isDragging = false;
        e.Handled = true;
    }

    public enum Direction
    {
        Vertical,
        Horizontal
    }
}
