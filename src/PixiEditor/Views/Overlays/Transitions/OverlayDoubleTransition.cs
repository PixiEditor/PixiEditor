using Avalonia.Animation.Easings;

namespace PixiEditor.Views.Overlays.Transitions;

internal class OverlayDoubleTransition : OverlayTransition
{
    public new double From
    {
        get => (double)base.From;
        set => base.From = value;
    }

    public new double To
    {
        get => (double)base.To;
        set => base.To = value;
    }

    protected override object Interpolate(double progress)
    {
        return From + (To - From) * progress;
    }

    public OverlayDoubleTransition(double durationSeconds, double from, double to, Easing? easing = null) : base(durationSeconds, from, to, easing)
    {

    }
}
