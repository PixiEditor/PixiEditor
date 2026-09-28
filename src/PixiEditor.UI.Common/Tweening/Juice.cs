using System.Diagnostics;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Reactive;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace PixiEditor.Helpers.UI;

public static class Juice
{
    private static readonly Dictionary<Control, IDisposable> Subscriptions = new();
    private static readonly ConditionalWeakTable<Control, CancellationTokenSource> AnimationTokens = new();

    public static readonly AttachedProperty<string?> AnimationsProperty =
        AvaloniaProperty.RegisterAttached<Control, string?>(
            "Animations",
            typeof(Juice));

    public static void SetAnimations(Control control, string? value) =>
        control.SetValue(AnimationsProperty, value);

    public static string? GetAnimations(Control control) =>
        control.GetValue(AnimationsProperty);

    static Juice()
    {
        AnimationsProperty.Changed.AddClassHandler<Control>((control, _) =>
            UpdateSubscription(control));
    }

    private static CancellationToken RestartAnimation(Control control)
    {
        if (AnimationTokens.TryGetValue(control, out var existing))
        {
            existing.Cancel();
            existing.Dispose();
            AnimationTokens.Remove(control);
        }

        var cts = new CancellationTokenSource();
        AnimationTokens.Add(control, cts);

        return cts.Token;
    }

    private static void UpdateSubscription(Control control)
    {
        UpdateSubscriptionInternal(control);
    }

    private static void UpdateSubscriptionInternal(Control control)
    {
        if (Subscriptions.Remove(control, out var old))
            old.Dispose();

        var value = GetAnimations(control);

        if (string.IsNullOrWhiteSpace(value))
            return;

        var subscriptions = new CompositeDisposable();

        foreach (var entry in value.Split(
                     ';',
                     StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = entry.Split(
                ':',
                2,
                StringSplitOptions.TrimEntries);

            if (parts.Length != 2)
                continue;

            var subscription = CreateSubscription(
                control,
                parts[0],
                parts[1]);

            if (subscription is not null)
                subscriptions.AddRange(subscription);
        }

        if (subscriptions.Count > 0)
            Subscriptions[control] = subscriptions;
    }

    private static IDisposable[] AddButtonSubscriptions(
        Control triggerControl,
        Control animationTarget)
    {
        return new IDisposable[]
        {
            triggerControl.AddDisposableHandler(
                InputElement.PointerExitedEvent,
                (_, _) => _ = Normal(animationTarget), handledEventsToo: true),
            triggerControl.AddDisposableHandler(
                InputElement.PointerPressedEvent,
                (_, _) => _ = Press(animationTarget), handledEventsToo: true),
            triggerControl.AddDisposableHandler(
                InputElement.PointerReleasedEvent,
                (_, _) => _ = Release(animationTarget), handledEventsToo: true)
        };
    }

    private static IDisposable[]? CreateSubscription(
        Control triggerControl,
        string trigger,
        string animation,
        Control? targetControl = null)
    {
        if (trigger.StartsWith("[") && trigger.Contains("]."))
        {
            int indexOfDot = trigger.IndexOf("].", StringComparison.Ordinal);
            var propertyName = trigger.Substring(1, indexOfDot - 1);
            trigger = trigger.Substring(indexOfDot + 2);

            bool lookForFirstParent = propertyName.StartsWith('^');
            Control? found = null;
            if (lookForFirstParent)
            {
                propertyName = propertyName.Substring(1);
                found = triggerControl.GetVisualAncestors().OfType<Control>()
                    .FirstOrDefault(c => c.Name == propertyName);
            }
            else
            {
                found = triggerControl.GetVisualDescendants().OfType<Control>()
                    .FirstOrDefault(c => c.Name == propertyName);
            }

            if (found != null)
            {
                targetControl = triggerControl;
                triggerControl = found;
            }
        }

        Control animationTarget = targetControl ?? triggerControl;


        if (animation.Equals("Button", StringComparison.OrdinalIgnoreCase))
        {
            return AddButtonSubscriptions(triggerControl, animationTarget);
        }

        return trigger switch
        {
            "PointerPressed" => [triggerControl.AddDisposableHandler(
                InputElement.PointerPressedEvent,
                (_, _) => Play(animationTarget, animation))],

            "PointerEntered" => [triggerControl.AddDisposableHandler(
                InputElement.PointerEnteredEvent,
                (_, _) => Play(animationTarget, animation))],

            "PointerExited" => [triggerControl.AddDisposableHandler(
                InputElement.PointerExitedEvent,
                (_, _) => Play(animationTarget, animation))],

            "PointerReleased" => [triggerControl.AddDisposableHandler(
                InputElement.PointerReleasedEvent,
                (_, _) => Play(animationTarget, animation))],

            "Loaded" => [triggerControl.AddDisposableHandler(
                Control.LoadedEvent,
                (_, _) => Play(animationTarget, animation))],

            "OnVisible" => [SubscribeProperty(triggerControl, Visual.IsVisibleProperty, (val) => val is true, animation)],
            "OnInvisible" => [SubscribeProperty(triggerControl, Visual.IsVisibleProperty, (val) => val is false,
                animation)],
            "Attached" => [SubscribeVisualTree(
                h => triggerControl.AttachedToVisualTree += h,
                h => triggerControl.AttachedToVisualTree -= h,
                () => Play(triggerControl, animation))],

            "Click" when triggerControl is Button button => [button.AddDisposableHandler(
                Button.ClickEvent,
                (_, _) => Play(triggerControl, animation),
                handledEventsToo: true)],

            _ => null
        };
    }

    private static IDisposable SubscribeVisualTree(
        Action<EventHandler<VisualTreeAttachmentEventArgs>> add,
        Action<EventHandler<VisualTreeAttachmentEventArgs>> remove,
        Action callback)
    {
        EventHandler<VisualTreeAttachmentEventArgs> handler = (_, _) => callback();

        add(handler);

        return new ActionDisposable(() => remove(handler));
    }

    private static IDisposable SubscribeProperty(
        Control control,
        AvaloniaProperty property,
        Func<object?, bool> predicate,
        string animation)
    {
        return property.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs>(change =>
        {
            if (change.Sender != control)
                return;

            if (predicate(change.NewValue))
                Play(control, animation);
        }));
    }

    private static void Play(Control control, string animation)
    {
        _ = animation switch
        {
            "Punch" => Punch(control),
            "Pop" => Pop(control),
            "PopIn" => PopIn(control),
            "Shake" => Shake(control),
            "Wiggle" => Wiggle(control),
            "Bounce" => Bounce(control),
            _ => Task.CompletedTask
        };
    }

    private static async Task Press(Control control)
    {
        var token = RestartAnimation(control);

        try
        {
            await ScaleTo(
                control,
                0.9,
                100,
                Easing.CubicEaseOut,
                token);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static async Task Normal(Control control)
    {
        var token = RestartAnimation(control);

        try
        {
            await ScaleTo(
                control,
                1.0,
                100,
                Easing.CubicEaseOut,
                token);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static async Task Release(Control control)
    {
        var token = RestartAnimation(control);

        try
        {
            await ScaleTo(
                control,
                1.1,
                100,
                Easing.CubicEaseIn,
                token);

            await ScaleTo(
                control,
                1.0,
                100,
                Easing.CubicEaseOut,
                token);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static Task ScaleFrom(
        Control control,
        double from,
        double to,
        double duration = 120,
        Easing? easing = null,
        CancellationToken cancellationToken = default)
    {
        var target = control;

        if (target is null)
            return Task.CompletedTask;

        var transform = GetScaleTransform(target);

        transform.ScaleX = from;
        transform.ScaleY = from;

        return Animate(
            target,
            duration,
            easing ?? Easing.CubicEaseOut,
            t =>
            {
                var value = from + (to - from) * t;
                transform.ScaleX = value;
                transform.ScaleY = value;
            },
            cancellationToken);
    }

    public static Task Scale(
        Control control,
        double scale,
        double duration = 120,
        Easing? easing = null,
        CancellationToken cancellationToken = default)
    {
        var target = control;

        if (target is null)
            return Task.CompletedTask;

        return Animate(
            target,
            duration,
            easing ?? Easing.CubicEaseOut,
            t =>
            {
                var transform = GetScaleTransform(target);

                transform.ScaleX = t * scale;
                transform.ScaleY = t * scale;
            },
            cancellationToken,
            1);
    }

    public static async Task ScaleTo(
        Control control,
        double scale,
        double duration = 100,
        Easing? easing = null,
        CancellationToken cancellationToken = default)
    {
        var target = control;

        if (target is null)
            return;

        var transform = GetScaleTransform(target);

        var startX = transform.ScaleX;
        var startY = transform.ScaleY;

        await Animate(
            target,
            duration,
            easing ?? Easing.CubicEaseOut,
            t =>
            {
                transform.ScaleX = startX + (scale - startX) * t;
                transform.ScaleY = startY + (scale - startY) * t;
            },
            cancellationToken);
    }

    public static async Task Punch(
        Control control,
        double amount = 1.08,
        double duration = 180,
        CancellationToken cancellationToken = default)
    {
        var half = duration / 2;

        await Scale(
            control,
            amount,
            half,
            Easing.CubicEaseOut,
            cancellationToken);

        await Scale(
            control,
            1,
            half,
            Easing.CubicEaseOut,
            cancellationToken);
    }

    public static Task PopIn(
        Control control,
        double duration = 180,
        CancellationToken cancellationToken = default)
    {
        return ScaleFrom(
            control,
            0,
            1,
            duration,
            Easing.BackEaseOut,
            cancellationToken);
    }

    public static async Task Pop(
        Control control,
        double amount = 1.08,
        double duration = 180,
        CancellationToken cancellationToken = default)
    {
        var target = control;

        if (target is null)
            return;

        var transform = GetScaleTransform(target);

        transform.ScaleX = 0;
        transform.ScaleY = 0;

        await Scale(
            control,
            amount,
            duration * 0.65,
            Easing.BackEaseOut,
            cancellationToken);

        await Scale(
            control,
            1,
            duration * 0.35,
            Easing.CubicEaseOut,
            cancellationToken);
    }

    public static Task Bounce(
        Control control,
        double amount = 12,
        double duration = 350,
        CancellationToken cancellationToken = default)
    {
        var target = control;

        if (target is null)
            return Task.CompletedTask;

        var transform = GetTranslateTransform(target);

        return Animate(
            target,
            duration,
            Easing.BounceEaseOut,
            t =>
            {
                transform.Y = amount * (1 - t);
            },
            cancellationToken);
    }

    public static Task Shake(
        Control control,
        double amount = 8,
        double duration = 300,
        CancellationToken cancellationToken = default)
    {
        var target = control;

        if (target is null)
            return Task.CompletedTask;

        var transform = GetTranslateTransform(target);

        return Animate(
            target,
            duration,
            Easing.CubicEaseOut,
            t =>
            {
                var x = Math.Sin(t * Math.PI * 8) * amount * (1 - t);
                transform.X = x;
            },
            cancellationToken);
    }

    public static Task Wiggle(
        Control control,
        double angle = 6,
        double duration = 300,
        CancellationToken cancellationToken = default)
    {
        var target = control;

        if (target is null)
            return Task.CompletedTask;

        var transform = GetRotateTransform(target);

        return Animate(
            target,
            duration,
            Easing.CubicEaseOut,
            t =>
            {
                transform.Angle = Math.Sin(t * Math.PI * 6) * angle * (1 - t);
            },
            cancellationToken);
    }

    public static Task Fade(
        Control control,
        double opacity,
        double duration = 120,
        Easing? easing = null,
        CancellationToken cancellationToken = default)
    {
        var target = control;

        if (target is null)
            return Task.CompletedTask;

        var from = target.Opacity;

        return Animate(
            target,
            duration,
            easing ?? Easing.CubicEaseOut,
            t =>
            {
                target.Opacity = from + (opacity - from) * t;
            },
            cancellationToken);
    }

    private static Task Animate(
        Control control,
        double duration,
        Easing easing,
        Action<double> update,
        CancellationToken cancellationToken,
        double from = 0)
    {
        if (duration <= 0)
        {
            update(1);
            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var stopwatch = Stopwatch.StartNew();

        DispatcherTimer? timer = null;

        void Stop()
        {
            timer?.Stop();
            timer = null;
        }

        void Tick(object? sender, EventArgs e)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                Stop();
                tcs.TrySetCanceled(cancellationToken);
                return;
            }

            var progress = Math.Clamp(
                stopwatch.Elapsed.TotalMilliseconds / duration,
                0,
                1);

            var value = easing.Ease(progress);

            update(from + (1 - from) * value);

            if (progress >= 1)
            {
                Stop();
                tcs.TrySetResult();
            }
        }

        timer = new DispatcherTimer(
            TimeSpan.FromMilliseconds(1000.0 / 60),
            DispatcherPriority.Render,
            Tick);

        timer.Start();

        return tcs.Task;
    }

    private static ScaleTransform GetScaleTransform(Control control)
    {
        if (control.RenderTransform is ScaleTransform scale)
            return scale;

        var transform = new ScaleTransform(1, 1);

        control.RenderTransform = transform;

        return transform;
    }

    private static TranslateTransform GetTranslateTransform(Control control)
    {
        if (control.RenderTransform is TranslateTransform translate)
            return translate;

        var transform = new TranslateTransform();

        control.RenderTransform = transform;

        return transform;
    }

    private static RotateTransform GetRotateTransform(Control control)
    {
        if (control.RenderTransform is RotateTransform rotate)
            return rotate;

        var transform = new RotateTransform();

        control.RenderTransform = transform;

        return transform;
    }
}
