using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;

namespace PixiEditor.AnimationRenderer.Core;

public struct Frame
{
    public Bitmap ImageData { get; set; }
    public int DurationTicks { get; set; }

    public Frame(Bitmap imageData, int durationTicks)
    {
        ImageData = imageData;
        DurationTicks = durationTicks;
    }
}
