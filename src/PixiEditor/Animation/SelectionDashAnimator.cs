using Avalonia.Animation;
using Avalonia.Media;

namespace PixiEditor.Animation;

public sealed class SelectionDashAnimator : InterpolatingAnimator<IDashPathEffect>
{
    
    public override IDashPathEffect Interpolate(double progress, IDashPathEffect oldValue, IDashPathEffect newValue)
    {
        return new DashPathEffect(oldValue.Dashes.ToArray(), (float)progress * 6);
    }

    public static IDashPathEffect Interpolate(double progress, int steps, float[] dashes)
    {
        var newDashStyle = new DashPathEffect(dashes, (float)progress * steps);
        return newDashStyle;
    }
}
