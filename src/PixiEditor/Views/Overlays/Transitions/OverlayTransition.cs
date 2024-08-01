using Avalonia.Animation.Easings;

namespace PixiEditor.Views.Overlays.Transitions;

public abstract class OverlayTransition
{
    public double DurationSeconds { get; set; }
    public Easing Easing { get; set; }
    public object From { get; set; }
    public object To { get; set; }
    public double Progress { get; set; }

    protected OverlayTransition(double durationSeconds, object from, object to, Easing? easing = null)
    {
        DurationSeconds = durationSeconds;
        Easing = easing ?? new LinearEasing();
        From = from;
        To = to;
    }

    protected abstract object Interpolate(double progress);

    public object Evaluate() => Evaluate(Progress);

    public object Evaluate(double progress)
    {
        double easedT = Easing.Ease(progress);
        return Interpolate(easedT);
    }
}
