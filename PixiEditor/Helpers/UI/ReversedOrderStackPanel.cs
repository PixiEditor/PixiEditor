using Avalonia;
using Avalonia.Controls;
using System;
using System.Linq;
using System.Windows;


namespace PixiEditor.Helpers.UI
{
    public class ReversedOrderStackPanel : StackPanel
    {
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            bool fHorizontal = Orientation == Avalonia.Layout.Orientation.Horizontal;
            var rcChild = new Rect(arrangeSize);
            double previousChildSize = 0.0;

            var children = Children.Cast<Control>().Reverse();
            foreach (Control child in children)
            {
                if (child == null)
                    continue;

                if (fHorizontal)
                {
                    rcChild = new Rect(rcChild.X + previousChildSize, rcChild.Y, rcChild.Width, rcChild.Height);
                    previousChildSize = child.DesiredSize.Width;
                    rcChild = new Rect(rcChild.X, rcChild.Y, previousChildSize, Math.Max(arrangeSize.Height, child.DesiredSize.Height));
                }
                else
                {
                    rcChild = new Rect(rcChild.X, rcChild.Y + previousChildSize, rcChild.Width, rcChild.Height);
                    previousChildSize = child.DesiredSize.Height;
                    rcChild = new Rect(rcChild.X, rcChild.Y, Math.Max(arrangeSize.Width, child.DesiredSize.Width), previousChildSize);
                }

                child.Arrange(rcChild);
            }

            return arrangeSize;
        }
    }
}