using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;

namespace PixiEditor.UI.Common.Behaviors;

public class TextBoxFocusBehavior : Behavior<TextBox>
{
    public static readonly StyledProperty<bool> SelectOnMouseClickProperty =
        AvaloniaProperty.Register<TextBoxFocusBehavior, bool>(
            nameof(SelectOnMouseClick));

    public static readonly StyledProperty<bool> ConfirmOnEnterProperty =
        AvaloniaProperty.Register<TextBoxFocusBehavior, bool>(
            nameof(ConfirmOnEnter));

    public static readonly StyledProperty<bool> DeselectOnFocusLossProperty =
        AvaloniaProperty.Register<TextBoxFocusBehavior, bool>(
            nameof(DeselectOnFocusLoss));

    public bool SelectOnMouseClick
    {
        get => (bool)GetValue(SelectOnMouseClickProperty);
        set => SetValue(SelectOnMouseClickProperty, value);
    }

    public bool ConfirmOnEnter
    {
        get => (bool)GetValue(ConfirmOnEnterProperty);
        set => SetValue(ConfirmOnEnterProperty, value);
    }
    public bool DeselectOnFocusLoss
    {
        get => (bool)GetValue(DeselectOnFocusLossProperty);
        set => SetValue(DeselectOnFocusLossProperty, value);
    }

    public static readonly StyledProperty<bool> FocusNextProperty =
        AvaloniaProperty.Register<TextBoxFocusBehavior, bool>(nameof(FocusNext));

    public bool FocusNext
    {
        get { return (bool)GetValue(FocusNextProperty); }
        set { SetValue(FocusNextProperty, value); }
    }

    public static IInputElement FallbackFocusElement { get; set; }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.GotFocus += AssociatedObjectGotKeyboardFocus;
        AssociatedObject.PointerPressed += AssociatedObjectGotMouseCapture;
        AssociatedObject.LostFocus += AssociatedObject_LostFocus;
        AssociatedObject.PointerPressed += OnPointerPressed;
        AssociatedObject.KeyUp += AssociatedObject_KeyUp;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.GotFocus -= AssociatedObjectGotKeyboardFocus;
        AssociatedObject.PointerPressed -= AssociatedObjectGotMouseCapture;
        AssociatedObject.LostFocus -= AssociatedObject_LostFocus;
        AssociatedObject.PointerPressed -= OnPointerPressed;
        AssociatedObject.KeyUp -= AssociatedObject_KeyUp;
    }

    // Converts number to proper format if enter is clicked and moves focus to next object
    private void AssociatedObject_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || !ConfirmOnEnter)
            return;

        RemoveFocus();
    }

    private void RemoveFocus()
    {
        var next = FocusNext
            ? KeyboardNavigationHandler.GetNext(AssociatedObject, NavigationDirection.Next)
            : FallbackFocusElement;
        NavigationMethod nextMethod = FocusNext ? NavigationMethod.Directional : NavigationMethod.Unspecified;
        if (next == AssociatedObject) return;
        next?.Focus(nextMethod);
    }

    private void AssociatedObjectGotKeyboardFocus(
        object sender,
        GotFocusEventArgs e)
    {
        if ((e.NavigationMethod == NavigationMethod.Pointer && SelectOnMouseClick) || e.NavigationMethod == NavigationMethod.Tab)
        {
            Dispatcher.UIThread.Post(() => AssociatedObject?.SelectAll(), DispatcherPriority.Input);
        }
    }

    private void AssociatedObjectGotMouseCapture(
        object? sender, PointerPressedEventArgs pointerPressedEventArgs)
    {
        if (SelectOnMouseClick)
        {
            Dispatcher.UIThread.Post(() => AssociatedObject?.SelectAll(), DispatcherPriority.Input);
        }
    }

    private void AssociatedObject_LostFocus(object sender, RoutedEventArgs e)
    {
        if (DeselectOnFocusLoss)
            AssociatedObject.ClearSelection();

        //RemoveFocus();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!SelectOnMouseClick)
            return;

        if (!AssociatedObject.IsKeyboardFocusWithin)
        {
            AssociatedObject.Focus();
            e.Handled = true;
        }
    }
}
