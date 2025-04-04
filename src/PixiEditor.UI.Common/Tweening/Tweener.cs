using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Threading;

namespace PixiEditor.UI.Common.Tweening;

public static class Tween
{
    public static Tweener<double> Double(AvaloniaProperty<double> property, Control control,
        double endValue, double duration, IEasing easing = null)
    {
        return new Tweener<double>(property, control, endValue, duration,
            (start, end, t) => start + (end - start) * easing?.Ease(t) ?? t);
    }
}

public class Tweener<T>
{
    public AvaloniaProperty<T> Property { get; }

    public T StartValue { get; private set; }
    public T EndValue { get; }
    public double DurationMs { get; }

    public Func<T, T, double, T> Interpolator { get; }

    public Control Control { get; set; }

    private DispatcherTimer timer;

    public Tweener(AvaloniaProperty<T> property, Control control, T endValue, double durationMs,
        Func<T, T, double, T> interpolator)
    {
        Property = property;
        EndValue = endValue;
        DurationMs = durationMs;
        Interpolator = interpolator;
        Control = control;
    }

    public Tweener<T> Run()
    {
        timer = new DispatcherTimer(DispatcherPriority.Default) { Interval = TimeSpan.FromMilliseconds(16) };
        DateTime startTime = DateTime.Now;
        StartValue = (T)Control.GetValue(Property);
        timer.Tick += (sender, args) =>
        {
            double elapsed = (DateTime.Now - startTime).TotalMilliseconds;
            if (elapsed >= DurationMs)
            {
                timer.Stop();
                Control.SetValue(Property, EndValue);
                return;
            }

            double t = elapsed / DurationMs;
            T value = Interpolator(StartValue, EndValue, t);
            Control.SetValue(Property, value);
        };

        timer.Start();
        return this;
    }

    public void Stop()
    {
        timer.Stop();
        Control.SetValue(Property, EndValue);
    }
}
