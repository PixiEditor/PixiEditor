using Avalonia.Animation;
using Avalonia.Media;

namespace PixiEditor.Animation;

public sealed class SelectionDashAnimator : InterpolatingAnimator<IDashStyle>
{
    public override IDashStyle Interpolate(double progress, IDashStyle oldValue, IDashStyle newValue)
    {
        return new DashStyle(oldValue.Dashes, progress * 6);
    }

    public static IDashStyle Interpolate(double progress, int steps, double[] dashes)
    {
        var newDashStyle = new DashStyle(dashes, progress * steps);
        return newDashStyle;
    }
}
