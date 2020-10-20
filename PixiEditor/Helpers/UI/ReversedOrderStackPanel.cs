using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PixiEditor.Helpers.UI
{
    public class ReversedOrderStackPanel : StackPanel
    {
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var fHorizontal = Orientation == Orientation.Horizontal;
            var rcChild = new Rect(arrangeSize);
            var previousChildSize = 0.0;

            var children = InternalChildren.Cast<UIElement>().Reverse();
            foreach (var child in children)
            {
                if (child == null)
                    continue;

                if (fHorizontal)
                {
                    rcChild.X += previousChildSize;
                    previousChildSize = child.DesiredSize.Width;
                    rcChild.Width = previousChildSize;
                    rcChild.Height = Math.Max(arrangeSize.Height, child.DesiredSize.Height);
                }
                else
                {
                    rcChild.Y += previousChildSize;
                    previousChildSize = child.DesiredSize.Height;
                    rcChild.Height = previousChildSize;
                    rcChild.Width = Math.Max(arrangeSize.Width, child.DesiredSize.Width);
                }

                child.Arrange(rcChild);
            }

            return arrangeSize;
        }
    }
}