using Avalonia;
using Avalonia.Controls;
using PixiEditor.Extensions.FlyUI.Elements;

namespace PixiEditor.Extensions.UI.Panels;

public class ColumnPanel : Panel
{
    public MainAxisAlignment MainAxisAlignment { get; set; } = MainAxisAlignment.Start;
    public CrossAxisAlignment CrossAxisAlignment { get; set; } = CrossAxisAlignment.Start;


    protected override Size MeasureOverride(Size availableSize)
    {
        Size size = new(0, 0);
        foreach (var child in Children)
        {
            child.Measure(availableSize);
            size += new Size(0, child.DesiredSize.Height);

            if (CrossAxisAlignment == CrossAxisAlignment.Stretch)
            {
                size = new Size(availableSize.Width, size.Height);
            }
            else
            {
                size = new Size(Math.Max(size.Width, child.DesiredSize.Width), size.Height);
            }
        }

        if (MainAxisAlignment == MainAxisAlignment.SpaceBetween)
        {
            size = new Size(size.Width, availableSize.Height);
        }
        else if (MainAxisAlignment == MainAxisAlignment.SpaceAround)
        {
            size = new Size(size.Width, availableSize.Height);
        }
        else if (MainAxisAlignment == MainAxisAlignment.SpaceEvenly)
        {
            size = new Size(size.Width, availableSize.Height);
        }

        return size;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double totalYSpace = 0;

        foreach (var child in Children)
        {
            totalYSpace += child.DesiredSize.Height;
        }

        bool stretchPlacement = MainAxisAlignment is MainAxisAlignment.SpaceBetween or MainAxisAlignment.SpaceAround
            or MainAxisAlignment.SpaceEvenly;
        double spaceBetween = 0;
        double spaceBeforeAfter = 0;

        if (stretchPlacement)
        {
            double freeSpace = finalSize.Height - totalYSpace;
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
            spaceBeforeAfter = (finalSize.Height - totalYSpace) / 2;
        }
        else if (MainAxisAlignment == MainAxisAlignment.End)
        {
            spaceBeforeAfter = finalSize.Height - totalYSpace;
        }

        double yOffset = spaceBeforeAfter;
        foreach (var child in Children)
        {
            double xOffset = 0;
            if (CrossAxisAlignment == CrossAxisAlignment.Center)
            {
                xOffset = finalSize.Width / 2f - child.DesiredSize.Width / 2f;
            }
            else if (CrossAxisAlignment == CrossAxisAlignment.End)
            {
                xOffset = finalSize.Width - child.DesiredSize.Width;
            }
            else if (CrossAxisAlignment == CrossAxisAlignment.Stretch)
            {
                xOffset = 0;
                child.Arrange(new Rect(0, yOffset, finalSize.Width, child.DesiredSize.Height));
                yOffset += child.DesiredSize.Height + spaceBetween;
                continue;
            }

            child.Arrange(new Rect(xOffset, yOffset, child.DesiredSize.Width, child.DesiredSize.Height));
            yOffset += child.DesiredSize.Height + spaceBetween;
        }

        return finalSize;
    }
}
