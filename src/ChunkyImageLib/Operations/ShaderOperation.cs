using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Surfaces.Surface;
using PixiEditor.DrawingApi.Core.Surfaces.Surface.PaintImpl;
using PixiEditor.Numerics;

namespace ChunkyImageLib.Operations;

public class ShaderOperation : IDrawOperation
{
    public bool IgnoreEmptyChunks { get; } = false;
    public Shader Shader { get; }
    
    
    public ShaderOperation(Shader shader)
    {
        Shader = shader;
    }

    public void DrawOnChunk(Chunk targetChunk, VecI chunkPos)
    {
        using Paint paint = new Paint();
        paint.Shader = Shader;
        targetChunk.Surface.DrawingSurface.Canvas.DrawPaint(paint);
    }

    public AffectedArea FindAffectedArea(VecI imageSize)
    {
        return new AffectedArea(OperationHelper.FindChunksTouchingRectangle(new RectI(VecI.Zero, imageSize), ChunkyImage.FullChunkSize));
    }

    public void Dispose()
    {
    }
}
