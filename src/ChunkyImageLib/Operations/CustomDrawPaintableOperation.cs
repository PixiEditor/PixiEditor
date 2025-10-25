using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;

namespace ChunkyImageLib.Operations;

internal class CustomDrawPaintableOperation : IDrawOperation
{
    public RectD Bounds { get; private set; }
    private Action<Canvas> drawAction;

    public CustomDrawPaintableOperation(Action<Canvas> customDrawAction, RectD bounds)
    {
        Bounds = bounds;
        drawAction = customDrawAction;
    }

    public bool IgnoreEmptyChunks => false;


    public void DrawOnChunk(Chunk targetChunk, VecI chunkPos)
    {
        var surf = targetChunk.Surface.DrawingSurface;

        surf.Canvas.Save();
        surf.Canvas.Scale((float)targetChunk.Resolution.Multiplier());
        surf.Canvas.Translate(-chunkPos * ChunkyImage.FullChunkSize);
        drawAction(surf.Canvas);
        surf.Canvas.Restore();
    }

    public AffectedArea FindAffectedArea(VecI imageSize)
    {
        return new AffectedArea(OperationHelper.FindChunksTouchingRectangle((RectI)Bounds, ChunkyImage.FullChunkSize), (RectI)Bounds);
    }

    public void Dispose()
    {
        
    }
}
