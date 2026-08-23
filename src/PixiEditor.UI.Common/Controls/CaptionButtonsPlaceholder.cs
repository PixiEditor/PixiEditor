using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Reactive;
using Avalonia.VisualTree;

namespace PixiEditor.UI.Common.Controls;

public class CaptionButtonsPlaceholder : TemplatedControl
{
    public static readonly StyledProperty<bool> CanMinimizeProperty =
        AvaloniaProperty.Register<CaptionButtonsPlaceholder, bool>(
            nameof(CanMinimize), true);

    public static readonly StyledProperty<bool> CanMaximizeProperty =
        AvaloniaProperty.Register<CaptionButtonsPlaceholder, bool>(
            nameof(CanMaximize), true);

    public bool CanMaximize
    {
        get => GetValue(CanMaximizeProperty);
        set => SetValue(CanMaximizeProperty, value);
    }

    public bool CanMinimize
    {
        get => GetValue(CanMinimizeProperty);
        set => SetValue(CanMinimizeProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (VisualRoot?.Parent is Window window)
        {
            IsVisible = window.IsExtendedIntoWindowDecorations;
            window.GetPropertyChangedObservable(Window.IsExtendedIntoWindowDecorationsProperty).Subscribe(
                new AnonymousObserver<AvaloniaPropertyChangedEventArgs>((
                    e) =>
                {
                    IsVisible = e.NewValue is true;
                }));
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        var bounds = VisualRoot.GetLogicalChildren().FirstOrDefault().FindLogicalDescendantOfType<WindowDrawnDecorationsContent>()
            .Overlay.GetVisualChildren().FirstOrDefault(x => x.Name == "PART_OverlayPanel").Bounds;
        Width = bounds.Width;
        Height = bounds.Height;
    }
}
