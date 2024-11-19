using Avalonia.Media;
using Avalonia.Styling;

namespace PixiEditor.Animation;

public class Animators : Styles
{
    public Animators(IServiceProvider? sp = null)
    {
        Avalonia.Animation.Animation.RegisterCustomAnimator<IDashPathEffect, SelectionDashAnimator>();
    }
}
