using System.Diagnostics;
using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

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
    (this IReadOnlyChunkyImage image, RectI fullResRegion, ChunkResolution resolution, DrawingSurface surface,
        VecD pos, Paint? paint = null, SamplingOptions? sampling = null)
    {
        DrawRegionOn(fullResRegion, resolution, surface, pos, image.DrawMostUpToDateChunkOn, paint, sampling);
    }

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
    public static void DrawMostUpToDateRegionOnWithAffected
    (this IReadOnlyChunkyImage image, RectI fullResRegion, ChunkResolution resolution, DrawingSurface surface,
        AffectedArea affectedArea, VecD pos, Paint? paint = null, SamplingOptions? sampling = null)
    {
        DrawRegionOn(fullResRegion, resolution, surface, pos, image.DrawMostUpToDateChunkOn,
            image.DrawCachedMostUpToDateChunkOn, affectedArea, paint, sampling);
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
    (this IReadOnlyChunkyImage image, RectI fullResRegion, ChunkResolution resolution, DrawingSurface surface,
        VecI pos, Paint? paint = null, SamplingOptions? samplingOptions = null)
    {
        DrawRegionOn(fullResRegion, resolution, surface, pos, image.DrawCommittedChunkOn, paint, samplingOptions);
    }

    private static void DrawRegionOn(
        RectI fullResRegion,
        ChunkResolution resolution,
        DrawingSurface surface,
        VecD pos,
        Func<VecI, ChunkResolution, DrawingSurface, VecD, Paint?, SamplingOptions?, bool> drawingFunc,
        Paint? paint = null, SamplingOptions? samplingOptions = null)
    {
        int count = surface.Canvas.Save();
        surface.Canvas.ClipRect(new RectD(pos, fullResRegion.Size));

        VecI chunkTopLeft = OperationHelper.GetChunkPos(fullResRegion.TopLeft, ChunkyImage.FullChunkSize);
        VecI chunkBotRight = OperationHelper.GetChunkPos(fullResRegion.BottomRight, ChunkyImage.FullChunkSize);
        VecI offsetFullRes = (chunkTopLeft * ChunkyImage.FullChunkSize) - fullResRegion.Pos;
        VecI offsetTargetRes = (VecI)(offsetFullRes * resolution.Multiplier());

        for (int j = chunkTopLeft.Y; j <= chunkBotRight.Y; j++)
        {
            for (int i = chunkTopLeft.X; i <= chunkBotRight.X; i++)
            {
                var chunkPos = new VecI(i, j);
                drawingFunc(chunkPos, resolution, surface,
                    offsetTargetRes + (chunkPos - chunkTopLeft) * resolution.PixelSize() + pos, paint,
                    samplingOptions);
            }
        }

        surface.Canvas.RestoreToCount(count);
    }

    private static void DrawRegionOn(
        RectI fullResRegion,
        ChunkResolution resolution,
        DrawingSurface surface,
        VecD pos,
        Func<VecI, ChunkResolution, DrawingSurface, VecD, Paint?, SamplingOptions?, bool> drawingFunc,
        Func<VecI, ChunkResolution, DrawingSurface, VecD, Paint?, SamplingOptions?, bool> quickDrawingFunc,
        AffectedArea area,
        Paint? paint = null, SamplingOptions? samplingOptions = null)
    {
        int count = surface.Canvas.Save();
        surface.Canvas.ClipRect(new RectD(pos, fullResRegion.Size));

        VecI chunkTopLeft = OperationHelper.GetChunkPos(fullResRegion.TopLeft, ChunkyImage.FullChunkSize);
        VecI chunkBotRight = OperationHelper.GetChunkPos(fullResRegion.BottomRight, ChunkyImage.FullChunkSize);
        VecI offsetFullRes = (chunkTopLeft * ChunkyImage.FullChunkSize) - fullResRegion.Pos;
        VecI offsetTargetRes = (VecI)(offsetFullRes * resolution.Multiplier());

        for (int j = chunkTopLeft.Y; j <= chunkBotRight.Y; j++)
        {
            for (int i = chunkTopLeft.X; i <= chunkBotRight.X; i++)
            {
                var chunkPos = new VecI(i, j);
                if (area.Chunks.Contains(chunkPos))
                {
                    drawingFunc(chunkPos, resolution, surface,
                        offsetTargetRes + (chunkPos - chunkTopLeft) * resolution.PixelSize() + pos, paint,
                        samplingOptions);
                }
                else
                {
                    quickDrawingFunc(chunkPos, resolution, surface,
                        offsetTargetRes + (chunkPos - chunkTopLeft) * resolution.PixelSize() + pos, paint,
                        samplingOptions);
                }
            }
        }

        surface.Canvas.RestoreToCount(count);
    }
}
