using Avalonia.Animation;
using Avalonia.Media;

namespace PixiEditor.AvaloniaUI.Animators;

public sealed class SelectionDashAnimator : InterpolatingAnimator<IDashStyle>
{
    public override IDashStyle Interpolate(double progress, IDashStyle oldValue, IDashStyle newValue)
    {
        return Interpolate(progress);
    }

    public static IDashStyle Interpolate(double progress)
    {
        var newDashStyle = new DashStyle(new double[] { 2, 4 }, progress * 6);
        return newDashStyle;
    }
}
