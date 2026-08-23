using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Reactive;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace PixiEditor.UI.Common.Controls;

public class CaptionButtonsPlaceholder : Control
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

    protected override void OnLoaded(RoutedEventArgs e)
    {
        if (VisualRoot?.Parent is Window window)
        {
            IsVisible = window.IsExtendedIntoWindowDecorations;
            window.GetPropertyChangedObservable(Window.IsExtendedIntoWindowDecorationsProperty).Subscribe(
                new AnonymousObserver<AvaloniaPropertyChangedEventArgs>((
                    e) =>
                {
                    IsVisible = e.NewValue is true;
                    Dispatcher.Post(UpdateBounds, DispatcherPriority.Render);
                }));
        }

        UpdateBounds();
    }

    private void UpdateBounds()
    {
        var bounds = VisualRoot.GetLogicalChildren()?.FirstOrDefault().FindLogicalDescendantOfType<WindowDrawnDecorationsContent>()?
            .Overlay.GetVisualChildren()?.FirstOrDefault(x => x.Name == "PART_OverlayPanel")?.Bounds ?? new Rect();
        Width = bounds.Width;
        Height = bounds.Height;
    }
}
