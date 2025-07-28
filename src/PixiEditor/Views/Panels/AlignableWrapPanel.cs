using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace PixiEditor.Views.Panels;

internal class AlignableWrapPanel : Panel
{
    public HorizontalAlignment HorizontalContentAlignment
    {
        get { return (HorizontalAlignment)GetValue(HorizontalContentAlignmentProperty); }
        set { SetValue(HorizontalContentAlignmentProperty, value); }
    }

    public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
        AvaloniaProperty.Register<AlignableWrapPanel, HorizontalAlignment>(
            nameof(HorizontalContentAlignment),
            HorizontalAlignment.Left);

    static AlignableWrapPanel()
    {
        AffectsArrange<AlignableWrapPanel>(HorizontalContentAlignmentProperty);
    }

    protected override Size MeasureOverride(Size constraint)
    {
        Size curLineSize = default;
        Size panelSize = default;

        Controls children = Children;

        for (int i = 0; i < children.Count; i++)
        {
            Control child = children[i];

            // Flow passes its own constraint to children
            child.Measure(constraint);
            Size sz = child.DesiredSize;

            if (curLineSize.Width + sz.Width > constraint.Width)
            {
                panelSize = panelSize.WithWidth(Math.Max(curLineSize.Width, panelSize.Width))
                .WithHeight(panelSize.Height + curLineSize.Height);
                curLineSize = sz;

                if (sz.Width > constraint.Width)
                {
                    panelSize = new Size(Math.Max(sz.Width, panelSize.Width), panelSize.Height + sz.Height);
                    curLineSize = default;
                }
            }
            else
            {
                curLineSize = curLineSize.WithWidth(curLineSize.Width + sz.Width)
                .WithHeight(Math.Max(sz.Height, curLineSize.Height));
            }
        }

        panelSize = panelSize.WithWidth(Math.Max(curLineSize.Width, panelSize.Width))
            .WithHeight(panelSize.Height + curLineSize.Height);

        return panelSize;
    }

    protected override Size ArrangeOverride(Size arrangeBounds)
    {
        int firstInLine = 0;
        Size curLineSize = default;
        double accumulatedHeight = 0;
        Controls children = this.Children;

        for (int i = 0; i < children.Count; i++)
        {
            Size sz = children[i].DesiredSize;

            if (curLineSize.Width + sz.Width > arrangeBounds.Width)
            {
                ArrangeLine(accumulatedHeight, curLineSize, arrangeBounds.Width, firstInLine, i);

                accumulatedHeight += curLineSize.Height;
                curLineSize = sz;

                if (sz.Width > arrangeBounds.Width)
                {
                    ArrangeLine(accumulatedHeight, sz, arrangeBounds.Width, i, ++i);
                    accumulatedHeight += sz.Height;
                    curLineSize = default;
                }

                firstInLine = i;
            }
            else
            {
                curLineSize = curLineSize.WithWidth(curLineSize.Width + sz.Width)
                    .WithHeight(Math.Max(sz.Height, curLineSize.Height));
            }
        }

        if (firstInLine < children.Count)
        {
            ArrangeLine(accumulatedHeight, curLineSize, arrangeBounds.Width, firstInLine, children.Count);
        }

        return arrangeBounds;
    }

    private void ArrangeLine(double y, Size lineSize, double boundsWidth, int start, int end)
    {
        double x = 0;
        if (this.HorizontalContentAlignment == HorizontalAlignment.Center)
        {
            x = (boundsWidth - lineSize.Width) / 2;
        }
        else if (this.HorizontalContentAlignment == HorizontalAlignment.Right)
        {
            x = boundsWidth - lineSize.Width;
        }

        Controls children = Children;
        for (int i = start; i < end; i++)
        {
            Control child = children[i];
            child.Arrange(new Rect(x, y, child.DesiredSize.Width, lineSize.Height));
            x += child.DesiredSize.Width;
        }
    }
}
