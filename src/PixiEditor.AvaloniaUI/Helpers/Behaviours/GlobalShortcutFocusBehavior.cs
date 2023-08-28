﻿using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using PixiEditor.AvaloniaUI.Models.Controllers;

namespace PixiEditor.AvaloniaUI.Helpers.Behaviours;

internal class GlobalShortcutFocusBehavior : Behavior<Control>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.GotFocus += AssociatedObject_GotKeyboardFocus;
        AssociatedObject.LostFocus += AssociatedObject_LostKeyboardFocus;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.GotFocus -= AssociatedObject_GotKeyboardFocus;
        AssociatedObject.LostFocus -= AssociatedObject_LostKeyboardFocus;
    }

    private void AssociatedObject_LostKeyboardFocus(object? sender, RoutedEventArgs routedEventArgs)
    {
        ShortcutController.UnblockShortcutExecution("GlobalShortcutFocusBehavior");
    }

    private void AssociatedObject_GotKeyboardFocus(object? sender, GotFocusEventArgs gotFocusEventArgs)
    {
        ShortcutController.BlockShortcutExecution("GlobalShortcutFocusBehavior");
    }
}