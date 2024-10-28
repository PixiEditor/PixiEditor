using Avalonia.Media;
using Avalonia.Styling;
using Drawie.Backend.Core.Surfaces.Vector;

namespace PixiEditor.Animation;

public class Animators : Styles
{
    public Animators(IServiceProvider? sp = null)
    {
        Avalonia.Animation.Animation.RegisterCustomAnimator<IDashPathEffect, SelectionDashAnimator>();
    }
}
