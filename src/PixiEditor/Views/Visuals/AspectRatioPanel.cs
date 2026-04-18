using Avalonia;
using Avalonia.Controls;

namespace PixiEditor.Views.Visuals;

public class AspectRatioPanel : Panel
{
    public static readonly StyledProperty<double> AspectWidthProperty = AvaloniaProperty.Register<AspectRatioPanel, double>(
        nameof(AspectWidth));

    public static readonly StyledProperty<double> AspectHeightProperty = AvaloniaProperty.Register<AspectRatioPanel, double>(
        nameof(AspectHeight));

    public double AspectHeight
    {
        get => GetValue(AspectHeightProperty);
        set => SetValue(AspectHeightProperty, value);
    }

    public double AspectWidth
    {
        get => GetValue(AspectWidthProperty);
        set => SetValue(AspectWidthProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var vidSize = new Size(AspectWidth, AspectHeight);
        if (double.IsFinite(availableSize.Width) || double.IsFinite(availableSize.Height))
        {
            double aspect = vidSize.Width / vidSize.Height;
            double width = double.IsFinite(availableSize.Width) ? availableSize.Width : vidSize.Width;
            double height = double.IsFinite(availableSize.Height) ? availableSize.Height : vidSize.Height;
            if (width / height > aspect)
            {
                width = height * aspect;
            }
            else
            {
                height = width / aspect;
            }

            foreach (var child in Children)
            {
                child.Measure(new Size(width, height));
            }

            return new Size(width, height);
        }

        return vidSize;
    }

    override protected Size ArrangeOverride(Size finalSize)
    {
        double aspect = (double)AspectWidth / AspectHeight;
        double width = finalSize.Width;
        double height = finalSize.Height;

        if (width / height > aspect)
        {
            width = height * aspect;
        }
        else
        {
            height = width / aspect;
        }

        foreach (var child in Children)
        {
            child.Arrange(new Rect(0, 0, width, height));
        }

        return new Size(width, height);
    }
}
