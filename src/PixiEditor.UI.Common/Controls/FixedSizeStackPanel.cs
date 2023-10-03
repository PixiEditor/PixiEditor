using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using OrientationEnum = Avalonia.Layout.Orientation;

namespace PixiEditor.UI.Common.Controls;

public class FixedSizeStackPanel : Panel
{
    public static readonly StyledProperty<double> ChildSizeProperty = 
        AvaloniaProperty.Register<FixedSizeStackPanel, double>(nameof(ChildSize), 10d);
    public double ChildSize
    {
        get => GetValue(ChildSizeProperty);
        set => SetValue(ChildSizeProperty, value);
    }
    
    public static readonly StyledProperty<double> SpacingProperty = 
        AvaloniaProperty.Register<FixedSizeStackPanel, double>(nameof(Spacing), 0d);
    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    public static readonly StyledProperty<OrientationEnum> OrientationProperty = 
        AvaloniaProperty.Register<FixedSizeStackPanel, OrientationEnum>(nameof(Orientation), OrientationEnum.Vertical);
    public OrientationEnum Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
    
    public static readonly StyledProperty<HorizontalAlignment> HorizontalChildrenAlignmentProperty = 
        AvaloniaProperty.Register<FixedSizeStackPanel, HorizontalAlignment>(nameof(Orientation), HorizontalAlignment.Left);
    public HorizontalAlignment HorizontalChildrenAlignment
    {
        get => GetValue(HorizontalChildrenAlignmentProperty);
        set => SetValue(HorizontalChildrenAlignmentProperty, value);
    }
    
    
    public static readonly StyledProperty<VerticalAlignment> VerticalChildrenAlignmentProperty = 
        AvaloniaProperty.Register<FixedSizeStackPanel, VerticalAlignment>(nameof(Orientation), VerticalAlignment.Center);
    public VerticalAlignment VerticalChildrenAlignment
    {
        get => GetValue(VerticalChildrenAlignmentProperty);
        set => SetValue(VerticalChildrenAlignmentProperty, value);
    }
    
    protected override Size MeasureOverride(Size availableSize)
    {
        Size childSize = Orientation switch
        {
            OrientationEnum.Horizontal => new Size(
                ChildSize, 
                FirstValid(availableSize.Height, Height, 0)),
            OrientationEnum.Vertical => new Size(
                FirstValid(availableSize.Width, Width, 0), 
                ChildSize),
            _ => throw new ArgumentOutOfRangeException()
        };
        
        foreach (var child in Children)
        {
            child.Measure(childSize);
        }
        
        double totalSize = Children.Count * ChildSize + Math.Max(Children.Count - 1, 0) * Spacing;
        return Orientation switch
        {
            OrientationEnum.Horizontal => new Size(
                Math.Min(totalSize, availableSize.Width), 
                FirstValid(availableSize.Height, Height, 0)),
            OrientationEnum.Vertical => new Size(
                FirstValid(availableSize.Width, Width, 0), 
                Math.Min(totalSize, availableSize.Height)),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private double FirstValid(double v1, double v2, double v3) => FirstValid(FirstValid(v1, v2), v3);
    
    private double FirstValid(double v1, double v2)
    {
        if (double.IsNaN(v1) || double.IsInfinity(v1))
            return v2;
        return v1;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        for (var i = 0; i < Children.Count; i++)
        {
            Control child = Children[i];
            double offset = (ChildSize + Spacing) * i;

            switch (Orientation)
            {
                case Orientation.Horizontal:
                    child.Arrange(
                        AlignChild(
                            new Rect(new Point(offset, 0), new Size(ChildSize, finalSize.Height)),
                            child.DesiredSize,
                            HorizontalChildrenAlignment,
                            VerticalChildrenAlignment)
                        );
                    break;
                case Orientation.Vertical:
                    child.Arrange(
                        AlignChild(
                                new Rect(new Point(0, offset), new Size(finalSize.Width, ChildSize)),
                                child.DesiredSize,
                                HorizontalChildrenAlignment,
                                VerticalChildrenAlignment)
                            );
                    break;
            }
        }

        return finalSize;
    }

    private Rect AlignChild(Rect area, Size childSize, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
    {
        double x = area.X + horizontalAlignment switch
        {
            HorizontalAlignment.Stretch or HorizontalAlignment.Left => 0,
            HorizontalAlignment.Center => (area.Size.Width - childSize.Width) / 2,
            HorizontalAlignment.Right => area.Size.Width - childSize.Width,
            _ => throw new ArgumentOutOfRangeException(nameof(horizontalAlignment), horizontalAlignment, null)
        };
        
        double y = area.Y + verticalAlignment switch
        {
            VerticalAlignment.Stretch or VerticalAlignment.Top => 0,
            VerticalAlignment.Center => (area.Size.Height - childSize.Height) / 2,
            VerticalAlignment.Bottom => area.Size.Height - childSize.Height,
            _ => throw new ArgumentOutOfRangeException(nameof(verticalAlignment), verticalAlignment, null)
        };

        double width = horizontalAlignment == HorizontalAlignment.Stretch ? area.Width : childSize.Width;
        double height = horizontalAlignment == HorizontalAlignment.Stretch ? area.Height : childSize.Height;

        return new Rect(x, y, width, height);
    }
}
