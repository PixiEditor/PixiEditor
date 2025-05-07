using Avalonia;
using Avalonia.Controls;
using PixiEditor.Extensions.FlyUI.Elements;

namespace PixiEditor.Extensions.UI.Panels;

public class RowPanel : Panel
{
    public MainAxisAlignment MainAxisAlignment { get; set; } = MainAxisAlignment.Start;
    public CrossAxisAlignment CrossAxisAlignment { get; set; } = CrossAxisAlignment.Start;


    protected override Size MeasureOverride(Size availableSize)
    {
        Size size = new(0, 0);
        foreach (var child in Children)
        {
            child.Measure(availableSize);
            size += new Size(child.DesiredSize.Width, 0);
            size = new Size(size.Width, Math.Max(size.Height, child.DesiredSize.Height));
        }

        if (MainAxisAlignment == MainAxisAlignment.SpaceBetween)
        {
            size = new Size(availableSize.Width, size.Height);
        }
        else if (MainAxisAlignment == MainAxisAlignment.SpaceAround)
        {
            size = new Size(availableSize.Width, size.Height);
        }
        else if (MainAxisAlignment == MainAxisAlignment.SpaceEvenly)
        {
            size = new Size (availableSize.Width, size.Height);
        }

        return size;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double totalXSpace = 0;

        foreach (var child in Children)
        {
            totalXSpace += child.DesiredSize.Width;
        }

        bool stretchPlacement = MainAxisAlignment is MainAxisAlignment.SpaceBetween or MainAxisAlignment.SpaceAround
            or MainAxisAlignment.SpaceEvenly;
        double spaceBetween = 0;
        double spaceBeforeAfter = 0;
        if (stretchPlacement)
        {
            double freeSpace = finalSize.Width - totalXSpace;
            spaceBetween = freeSpace / (Children.Count - 1);

            if (MainAxisAlignment == MainAxisAlignment.SpaceAround)
            {
                spaceBetween = freeSpace / Children.Count;
                spaceBeforeAfter = spaceBetween / 2f;
            }
            else if (MainAxisAlignment == MainAxisAlignment.SpaceEvenly)
            {
                spaceBeforeAfter = freeSpace / (Children.Count + 1);
                spaceBetween = (freeSpace - spaceBeforeAfter) / Children.Count;
            }
        }
        else if (MainAxisAlignment == MainAxisAlignment.Center)
        {
            spaceBeforeAfter = (finalSize.Width - totalXSpace) / 2;
        }
        else if (MainAxisAlignment == MainAxisAlignment.End)
        {
            spaceBeforeAfter = finalSize.Width - totalXSpace;
        }

        double xOffset = spaceBeforeAfter;
        foreach (var child in Children)
        {
            double yOffset = 0;
            if (CrossAxisAlignment == CrossAxisAlignment.Center)
            {
                yOffset = finalSize.Height / 2f - child.DesiredSize.Height / 2f;
            }
            else if (CrossAxisAlignment == CrossAxisAlignment.End)
            {
                yOffset = finalSize.Height - child.DesiredSize.Height;
            }

            child.Arrange(new Rect(xOffset, yOffset, child.DesiredSize.Width, child.DesiredSize.Height));
            xOffset += child.DesiredSize.Width + spaceBetween;
        }

        return finalSize;
    }
}
