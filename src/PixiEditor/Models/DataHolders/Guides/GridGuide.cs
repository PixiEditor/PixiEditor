using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.Views.UserControls.Guides;

namespace PixiEditor.Models.DataHolders.Guides;

internal class GridGuide : Guide
{
    private DocumentViewModel document;
    private double verticalOffset;
    private double horizontalOffset;
    private Color horizontalColor;
    private Color verticalColor;
    private bool isGrabbingPoint;
    
    public double VerticalOffset
    {
        get => verticalOffset;
        set
        {
            if (SetProperty(ref verticalOffset, value))
            {
                InvalidateVisual();
            }
        }
    }

    public double HorizontalOffset
    {
        get => horizontalOffset;
        set
        {
            if (SetProperty(ref horizontalOffset, value))
            {
                InvalidateVisual();
            }
        }
    }

    public Color HorizontalColor
    {
        get => horizontalColor;
        set
        {
            if (SetProperty(ref horizontalColor, value))
            {
                InvalidateVisual();
            }
        }
    }

    public Color VerticalColor
    {
        get => verticalColor;
        set
        {
            if (SetProperty(ref verticalColor, value))
            {
                InvalidateVisual();
            }
        }
    }
    
    public GridGuide(DocumentViewModel document) : base(document)
    {
        SettingsControl = new GridGuideSettings(this);
    }

    public override Control SettingsControl { get; }
    
    public override string TypeNameKey => "GRID_GUIDE";
    
    public override void Draw(DrawingContext context, GuideRenderer renderer)
    {
        double sizeMod = (ShowExtended, IsEditing) switch
        {
            (false, false) => 1,
            (true, false) => 1.5,
            (_, true) => 3
        } * renderer.ScreenUnit;

        if (ShowExtended || IsEditing)
        {
            context.DrawEllipse(Brushes.Aqua, null, new Point(VerticalOffset, HorizontalOffset), sizeMod * 2, sizeMod * 2);
        }
        
        if (HorizontalOffset == 0 || VerticalOffset == 0)
        {
            return;
        }

        var size = Document.SizeBindable - VecI.One;

        var verticalCount = (int)Math.Round(size.X / VerticalOffset, MidpointRounding.AwayFromZero);
        var horizontalCount = (int)Math.Round(size.Y / HorizontalOffset, MidpointRounding.AwayFromZero);

        var verticalPen = new Pen(new SolidColorBrush(VerticalColor), sizeMod);
        var horizontalPen = new Pen(new SolidColorBrush(HorizontalColor), sizeMod);

        for (var x = 1; x <= verticalCount; x++)
        {
            double offset = x * VerticalOffset;
            
            if (size.X + 1 > offset)
            {
                context.DrawLine(verticalPen, new Point(offset, 0), new Point(offset, Document.SizeBindable.Y));
            }
        }
        
        for (var y = 1; y <= horizontalCount; y++)
        {
            double offset = y * HorizontalOffset;
            if (size.Y + 1 > offset)
            {
                context.DrawLine(horizontalPen, new Point(0, offset), new Point(Document.SizeBindable.X, offset));
            }
        }

        if (ShowExtended || IsEditing)
        {
            context.DrawEllipse(Brushes.Aqua, null, new Point(VerticalOffset, HorizontalOffset), sizeMod * 2, sizeMod * 2);
        }
    }

    protected override void RendererAttached(GuideRenderer renderer)
    {
        renderer.MouseEnter += RendererOnMouseEnter;
        renderer.MouseLeave += RendererOnMouseLeave;
        
        renderer.MouseLeftButtonDown += RendererOnMouseLeftButtonDown;
        renderer.MouseMove += RendererOnMouseMove;
        renderer.MouseLeftButtonUp += RendererOnMouseLeftButtonUp;
    }

    private void RendererOnMouseEnter(object sender, MouseEventArgs e)
    {
        var renderer = (GuideRenderer)sender;

        if (!IsEditing || !IsInEditingPoint(renderer, e))
        {
            return;
        }

        renderer.Cursor = Cursors.Cross;
        e.Handled = true;
    }
    
    private void RendererOnMouseLeave(object sender, MouseEventArgs e)
    {
        var renderer = (GuideRenderer)sender;

        renderer.Cursor = null;
    }
    
    private void RendererOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var renderer = (GuideRenderer)sender;

        if (!IsEditing || !IsInEditingPoint(renderer, e))
        {
            return;
        }

        isGrabbingPoint = true;
        Mouse.Capture(renderer);
        e.Handled = true;
    }

    private void RendererOnMouseMove(object sender, MouseEventArgs e)
    {
        var renderer = (GuideRenderer)sender;
        
        if (IsEditing && IsInEditingPoint(renderer, e))
        {
            renderer.Cursor = Cursors.Cross;
        }
        else
        {
            renderer.Cursor = null;
        }

        if (!isGrabbingPoint)
        {
            return;
        }

        e.Handled = true;
        var point = e.GetPosition(renderer);

        VerticalOffset = Math.Round(point.X);
        HorizontalOffset = Math.Round(point.Y);
    }

    private void RendererOnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!isGrabbingPoint)
        {
            return;
        }

        Mouse.Capture(null);
        e.Handled = true;
        isGrabbingPoint = false;
    }

    private bool IsInEditingPoint(GuideRenderer renderer, MouseEventArgs args)
    {
        var offset = new Point(VerticalOffset, HorizontalOffset) - args.GetPosition(renderer);
        return offset.Length < 6 * renderer.ScreenUnit;
    }
}
