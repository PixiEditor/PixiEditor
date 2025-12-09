using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace PixiEditor.UI.Common.Controls;

public class SelectableStrip : Panel
{
    public static readonly AttachedProperty<bool> IsStripSelectedProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>("IsStripSelected", typeof(SelectableStrip));

    public static void SetIsStripSelected(AvaloniaObject obj, bool value) =>
        obj.SetValue(IsStripSelectedProperty, value);

    public static bool GetIsStripSelected(AvaloniaObject obj) =>
        obj.GetValue(IsStripSelectedProperty);

    public double HighlightX
    {
        get => GetValue(HighlightXProperty);
        set => SetValue(HighlightXProperty, value);
    }

    public static readonly StyledProperty<double> HighlightXProperty =
        AvaloniaProperty.Register<SelectableStrip, double>(nameof(HighlightX));

    private Border _highlight;

    static SelectableStrip()
    {
        HighlightXProperty.Changed.AddClassHandler<SelectableStrip>((strip, e) =>
        {
            if (e.Property != HighlightXProperty)
                return;

            if (strip._highlight.RenderTransform is TranslateTransform transform)
            {
                transform.X = (double)e.NewValue;
            }
        });

        IsStripSelectedProperty.Changed.AddClassHandler<Control>(OnSelectionChanged);
    }

    public SelectableStrip()
    {
        IBrush border = Brushes.Red;
        IBrush background = Brushes.Transparent;
        if (Application.Current.Styles.TryGetResource("ThemeBorderMidBrush", null, out object resource))
        {
            border = resource as IBrush;
        }

        if (Application.Current.Styles.TryGetResource("ThemeBorderLowBrush", null, out object bgResource))
        {
            background = bgResource as IBrush;
        }

        _highlight = new Border
        {
            Background = background, CornerRadius = new CornerRadius(4), ZIndex = -1,
            BorderBrush = border, BorderThickness = new Thickness(1),
        };

        _highlight.RenderTransform = new TranslateTransform();
        Transitions = new Transitions()
        {
            new DoubleTransition
            {
                Property = HighlightXProperty,
                Duration = TimeSpan.FromMilliseconds(160),
                Easing = new CubicEaseOut(),
            }
        };
    }

    public override void ApplyTemplate()
    {
        base.ApplyTemplate();
        if (!Children.Contains(_highlight))
        {
            Children.Insert(0, _highlight);
        }

        FindSelectedItem();
    }

    private static void OnSelectionChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != IsStripSelectedProperty)
            return;

        var control = e.Sender as Control;
        if (control is null || !GetIsStripSelected(control))
            return;

        var strip = control.GetVisualParent();
        while (strip != null && strip is not SelectableStrip)
        {
            strip = strip.GetVisualParent();
        }

        if (strip is not SelectableStrip selectableStrip)
            return;

        var pos = control.TranslatePoint(new Point(0, 0), selectableStrip) ?? new Point();
        selectableStrip.HighlightX = pos.X;
    }

    private void FindSelectedItem()
    {
        foreach (var child in Children)
        {
            if (child is ContentPresenter presenter && presenter.Child != null && GetIsStripSelected(presenter.Child))
            {
                var pos = presenter.Child.TranslatePoint(new Point(0, 0), this) ?? new Point();
                HighlightX = pos.X;
                break;
            }
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double x = 0;
        foreach (var child in Children)
        {
            if (child == _highlight)
            {
                child.Arrange(new Rect(new Point(0, 0), new Size(finalSize.Height, finalSize.Height)));
                continue;
            }

            child.Arrange(new Rect(new Point(x, 0), new Size(child.DesiredSize.Width, finalSize.Height)));

            x += child.DesiredSize.Width;
        }

        return finalSize;
    }

    override protected Size MeasureOverride(Size availableSize)
    {
        double totalWidth = 0;
        double maxHeight = 0;

        foreach (var child in Children)
        {
            if (child == _highlight)
            {
                continue;
            }

            child.Measure(availableSize);
            totalWidth += child.DesiredSize.Width;
            maxHeight = Math.Max(maxHeight, child.DesiredSize.Height);
        }

        return new Size(totalWidth, maxHeight);
    }
}
