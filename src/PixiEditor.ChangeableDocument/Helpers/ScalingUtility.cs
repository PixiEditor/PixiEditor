using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;

namespace PixiEditor.Helpers.UI;

public static class ScalingUtility
{
    public static void ScaleUniform(Canvas canvas, VecD source, VecD target)
    {
        float scaleX = (float)source.X / (float)target.X;
        float scaleY = (float)source.Y / (float)target.Y;
        var scale = Math.Min(scaleX, scaleY);
        float dX = (float)source.X / 2f / scale - (float)target.X / 2f;
        float dY = (float)source.Y / 2f / scale - (float)target.Y / 2f;
        canvas.Scale(scale, scale);
        canvas.Translate(dX, dY);
    }
}
