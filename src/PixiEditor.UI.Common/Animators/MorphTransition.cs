using Avalonia.Animation;
using Avalonia.Media;
using PixiEditor.UI.Common.Extensions;
using PixiEditor.UI.Common.Utilities;

namespace PixiEditor.UI.Common.Animators;

public class MorphTransition : InterpolatingTransitionBase<Geometry>
{
    protected override Geometry Interpolate(double progress, Geometry from, Geometry to)
    {
        var clone = (from as PathGeometry).ClonePathGeometry();

        Morph.To(clone, to as PathGeometry, progress);

        return clone;
    }
}
