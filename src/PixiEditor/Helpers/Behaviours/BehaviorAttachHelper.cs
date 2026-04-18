using Avalonia;
using Avalonia.Xaml.Interactions.Custom;
using Avalonia.Xaml.Interactivity;
using PixiEditor.UI.Common.Extensions;

namespace PixiEditor.Helpers.Behaviours;

public class BehaviorAttachHelper
{
    public static readonly AttachedProperty<bool> EnableGlobalShortcutFocusBehaviorProperty =
        AvaloniaProperty.RegisterAttached<BehaviorAttachHelper, AvaloniaObject, bool>(
            "EnableGlobalShortcutFocusBehavior");

    public static void SetEnableGlobalShortcutFocusBehavior(AvaloniaObject obj, bool value) =>
        obj.SetValue(EnableGlobalShortcutFocusBehaviorProperty, value);

    public static bool GetEnableGlobalShortcutFocusBehavior(AvaloniaObject obj) =>
        obj.GetValue(EnableGlobalShortcutFocusBehaviorProperty);

    public static readonly AttachedProperty<bool> EnableAutoFocusProperty =
        AvaloniaProperty.RegisterAttached<BehaviorAttachHelper, AvaloniaObject, bool>("EnableAutoFocus");

    public static void SetEnableAutoFocus(AvaloniaObject obj, bool value) =>
        obj.SetValue(EnableAutoFocusProperty, value);

    public static bool GetEnableAutoFocus(AvaloniaObject obj) => obj.GetValue(EnableAutoFocusProperty);

    static BehaviorAttachHelper()
    {
        EnableGlobalShortcutFocusBehaviorProperty.Changed.AddClassHandler<AvaloniaObject>(
            Attach<GlobalShortcutFocusBehavior>);
        EnableAutoFocusProperty.Changed.AddClassHandler<AvaloniaObject>((o, args) =>
        {
            var behaviors = Interaction.GetBehaviors(o);
            if ((bool)args.NewValue)
            {
                if (!behaviors.OfType<AutoFocusBehavior>().Any())
                {
                    behaviors.Add(new AutoFocusBehavior());
                }
            }
        });
    }

    private static void Attach<T>(AvaloniaObject obj, AvaloniaPropertyChangedEventArgs args) where T : Behavior, new()
    {
        var behaviors = Interaction.GetBehaviors(obj);
        if ((bool)args.NewValue)
        {
            if (!behaviors.OfType<T>().Any())
            {
                behaviors.Add(new T());
            }
        }
    }
}
