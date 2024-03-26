// https://github.com/wieslawsoltes/MorphingDemo/blob/main/MorphingDemo/Avalonia/MorphAnimator.cs
using Avalonia.Animation;
using Avalonia.Media;
using PixiEditor.UI.Common.Extensions;
using PixiEditor.UI.Common.Utilities;

namespace PixiEditor.UI.Common.Animators
{
    public class MorphAnimator : InterpolatingAnimator<Geometry>
    {
        public override Geometry Interpolate(double progress, Geometry oldValue, Geometry newValue)
        {
            var clone = (oldValue as PathGeometry).ClonePathGeometry();

            Morph.To(clone, newValue as PathGeometry, progress);

            return clone;
        }
    }
}
