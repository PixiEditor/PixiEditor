using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using SkiaSharp;

namespace PixiEditor.Models.Undo;

public class LayerChunk
{
    public Layer Layer { get; set; }
    public SKRectI AbsoluteChunkRect { get; set; }

    public LayerChunk(Layer layer, SKRectI absoluteChunkRect)
    {
        Layer = layer;
        AbsoluteChunkRect = absoluteChunkRect;
    }

}