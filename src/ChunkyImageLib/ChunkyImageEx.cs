using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace ChunkyImageLib;
public static class IReadOnlyChunkyImageEx
{
    /// <summary>
    /// Extracts a region from the <see cref="ChunkyImage"/> and draws it onto the passed <see cref="DrawingSurface"/>.
    /// The region is taken from the most up to date version of the <see cref="ChunkyImage"/>
    /// </summary>
    /// <param name="image"><see cref="ChunkyImage"/> to extract the region from</param>
    /// <param name="fullResRegion">The region to extract</param>
    /// <param name="resolution">Chunk resolution</param>
    /// <param name="surface">Surface to draw onto</param>
    /// <param name="pos">Starting position on the surface</param>
    /// <param name="paint">Paint to use for drawing</param>
    public static void DrawMostUpToDateRegionOn
        (this IReadOnlyChunkyImage image, RectI fullResRegion, ChunkResolution resolution, DrawingSurface surface, VecI pos, Paint? paint = null)
    {
        DrawRegionOn(fullResRegion, resolution, surface, pos, image.DrawMostUpToDateChunkOn, paint);
    }
    
    /// <summary>
    /// Extracts a region from the <see cref="ChunkyImage"/> and draws it onto the passed <see cref="DrawingSurface"/>.
    /// The region is taken from the committed version of the <see cref="ChunkyImage"/>
    /// </summary>
    /// <param name="image"><see cref="ChunkyImage"/> to extract the region from</param>
    /// <param name="fullResRegion">The region to extract</param>
    /// <param name="resolution">Chunk resolution</param>
    /// <param name="surface">Surface to draw onto</param>
    /// <param name="pos">Starting position on the surface</param>
    /// <param name="paint">Paint to use for drawing</param>
    public static void DrawCommittedRegionOn
        (this IReadOnlyChunkyImage image, RectI fullResRegion, ChunkResolution resolution, DrawingSurface surface, VecI pos, Paint? paint = null)
    {
        DrawRegionOn(fullResRegion, resolution, surface, pos, image.DrawCommittedChunkOn, paint);
    }
    
    private static void DrawRegionOn(
        RectI fullResRegion,
        ChunkResolution resolution,
        DrawingSurface surface,
        VecI pos,
        Func<VecI, ChunkResolution, DrawingSurface, VecI, Paint?, bool> drawingFunc,
        Paint? paint = null)
    {
        int count = surface.Canvas.Save();
        surface.Canvas.ClipRect(RectD.Create(pos, fullResRegion.Size));

        VecI chunkTopLeft = OperationHelper.GetChunkPos(fullResRegion.TopLeft, ChunkyImage.FullChunkSize);
        VecI chunkBotRight = OperationHelper.GetChunkPos(fullResRegion.BottomRight, ChunkyImage.FullChunkSize);
        VecI offsetFullRes = (chunkTopLeft * ChunkyImage.FullChunkSize) - fullResRegion.Pos;
        VecI offsetTargetRes = (VecI)(offsetFullRes * resolution.Multiplier());

        for (int j = chunkTopLeft.Y; j <= chunkBotRight.Y; j++)
        {
            for (int i = chunkTopLeft.X; i <= chunkBotRight.X; i++)
            {
                var chunkPos = new VecI(i, j);
                drawingFunc(chunkPos, resolution, surface, offsetTargetRes + (chunkPos - chunkTopLeft) * resolution.PixelSize() + pos, paint);
            }
        }

        surface.Canvas.RestoreToCount(count);
    }
}
