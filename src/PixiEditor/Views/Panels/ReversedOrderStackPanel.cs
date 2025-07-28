using System.Linq;
using Avalonia;
using Avalonia.Controls;

namespace PixiEditor.Views.Panels;

internal class ReversedOrderStackPanel : StackPanel
{
    protected override Size ArrangeOverride(Size arrangeSize)
    {
        bool fHorizontal = Orientation == Avalonia.Layout.Orientation.Horizontal;
        Rect rcChild = new Rect(arrangeSize);
        double previousChildSize = 0.0;

        System.Collections.Generic.IEnumerable<Control> children = Children.Reverse();
        foreach (Control child in children)
        {
            if (child == null)
            {
                continue;
            }

            if (fHorizontal)
            {
                rcChild = rcChild.WithX(rcChild.X + previousChildSize);
                previousChildSize = child.DesiredSize.Width;
                rcChild = rcChild.WithWidth(previousChildSize).WithHeight(Math.Max(arrangeSize.Height, child.DesiredSize.Height));
            }
            else
            {
                rcChild = rcChild.WithY(rcChild.Y + previousChildSize);
                previousChildSize = child.DesiredSize.Height;
                rcChild = rcChild.WithHeight(previousChildSize)
                    .WithWidth(Math.Max(arrangeSize.Width, child.DesiredSize.Width));
            }

            child.Arrange(rcChild);
        }

        return arrangeSize;
    }
}
