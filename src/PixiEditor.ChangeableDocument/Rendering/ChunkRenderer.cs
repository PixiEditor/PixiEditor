using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using OneOf;
using OneOf.Types;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Enums;
using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Rendering;

public static class ChunkRenderer
{
    private static SKPaint PaintToDrawChunksWith = new SKPaint() { BlendMode = SKBlendMode.SrcOver };
    private static SKPaint ReplacingPaint = new SKPaint() { BlendMode = SKBlendMode.Src };
    private static SKPaint ClippingPaint = new SKPaint() { BlendMode = SKBlendMode.DstIn };
    public static Chunk RenderWholeStructure(VecI pos, ChunkResolution resolution, IReadOnlyFolder root)
    {
        return RenderChunkRecursively(pos, resolution, 0, root, null);
    }

    public static Chunk RenderSpecificLayers(VecI pos, ChunkResolution resolution, IReadOnlyFolder root, HashSet<Guid> layers)
    {
        return RenderChunkRecursively(pos, resolution, 0, root, layers);
    }

    private static SKBlendMode GetSKBlendMode(BlendMode blendMode)
    {
        return blendMode switch
        {
            BlendMode.Normal => SKBlendMode.SrcOver,
            BlendMode.Darken => SKBlendMode.Darken,
            BlendMode.Multiply => SKBlendMode.Multiply,
            BlendMode.ColorBurn => SKBlendMode.ColorBurn,
            BlendMode.Lighten => SKBlendMode.Lighten,
            BlendMode.Screen => SKBlendMode.Screen,
            BlendMode.ColorDodge => SKBlendMode.ColorDodge,
            BlendMode.LinearDodge => SKBlendMode.Plus,
            BlendMode.Overlay => SKBlendMode.Overlay,
            BlendMode.SoftLight => SKBlendMode.SoftLight,
            BlendMode.HardLight => SKBlendMode.HardLight,
            BlendMode.Difference => SKBlendMode.Difference,
            BlendMode.Exclusion => SKBlendMode.Exclusion,
            BlendMode.Hue => SKBlendMode.Hue,
            BlendMode.Saturation => SKBlendMode.Saturation,
            BlendMode.Luminosity => SKBlendMode.Luminosity,
            BlendMode.Color => SKBlendMode.Color,
            _ => SKBlendMode.SrcOver,
        };
    }

    private static Chunk RenderChunkRecursively(VecI chunkPos, ChunkResolution resolution, int depth, IReadOnlyFolder folder, HashSet<Guid>? visibleLayers)
    {
        // if you are a skilled programmer any problem can be solved with enough if/else statements
        Chunk targetChunk = Chunk.Create(resolution);
        targetChunk.Surface.SkiaSurface.Canvas.Clear();

        //<clipping Chunk; None to clip with (fully masked out); No active clip>
        OneOf<Chunk, None, No> clippingChunk = new No();
        for (int i = 0; i < folder.ReadOnlyChildren.Count; i++)
        {
            var child = folder.ReadOnlyChildren[i];

            // next child might use clip to member below in which case we need to save the clip image
            bool needToSaveClip =
                i < folder.ReadOnlyChildren.Count - 1 &&
                !child.ClipToMemberBelow &&
                folder.ReadOnlyChildren[i + 1].ClipToMemberBelow;
            bool clipActiveWithReference = (clippingChunk.IsT0 || clippingChunk.IsT1) && child.ClipToMemberBelow;
            if (!child.ClipToMemberBelow && !clippingChunk.IsT2)
            {
                if (clippingChunk.IsT0)
                    clippingChunk.AsT0.Dispose();
                clippingChunk = new No();
            }

            if (!child.IsVisible)
            {
                if (needToSaveClip)
                    clippingChunk = new None();
                continue;
            }

            //// actual drawing
            // chunk fully masked out
            if (child.ReadOnlyMask is not null && !child.ReadOnlyMask.LatestOrCommittedChunkExists(chunkPos))
            {
                if (needToSaveClip)
                    clippingChunk = new None();
                continue;
            }

            // layer
            if (child is IReadOnlyLayer layer && (visibleLayers is null || visibleLayers.Contains(layer.GuidValue)))
            {
                // no mask
                if (layer.ReadOnlyMask is null)
                {
                    PaintToDrawChunksWith.Color = new SKColor(255, 255, 255, (byte)Math.Round(child.Opacity * 255));
                    PaintToDrawChunksWith.BlendMode = GetSKBlendMode(layer.BlendMode);
                    // draw while saving clip for later
                    if (needToSaveClip)
                    {
                        var clip = Chunk.Create(resolution);
                        if (!layer.ReadOnlyLayerImage.DrawMostUpToDateChunkOn(chunkPos, resolution, clip.Surface.SkiaSurface, new(0, 0), ReplacingPaint))
                        {
                            clip.Dispose();
                            clippingChunk = new None();
                            continue;
                        }
                        targetChunk.Surface.SkiaSurface.Canvas.DrawSurface(clip.Surface.SkiaSurface, new(0, 0), PaintToDrawChunksWith);
                        clippingChunk = clip;
                    }
                    // draw using saved clip
                    else if (clipActiveWithReference)
                    {
                        if (clippingChunk.IsT1)
                            continue;
                        using var tempChunk = Chunk.Create();
                        if (!layer.ReadOnlyLayerImage.DrawMostUpToDateChunkOn
                            (chunkPos, resolution, tempChunk.Surface.SkiaSurface, new(0, 0), ReplacingPaint))
                            continue;
                        OperationHelper.ClampAlpha(tempChunk.Surface.SkiaSurface, clippingChunk.AsT0.Surface.SkiaSurface);
                        targetChunk.Surface.SkiaSurface.Canvas.DrawSurface(tempChunk.Surface.SkiaSurface, new(0, 0), PaintToDrawChunksWith);
                    }
                    // just draw
                    else
                    {
                        layer.ReadOnlyLayerImage.DrawMostUpToDateChunkOn
                            (chunkPos, resolution, targetChunk.Surface.SkiaSurface, new(0, 0), PaintToDrawChunksWith);
                    }
                }
                // with mask
                else
                {
                    PaintToDrawChunksWith.Color = new SKColor(255, 255, 255, (byte)Math.Round(child.Opacity * 255));
                    PaintToDrawChunksWith.BlendMode = GetSKBlendMode(layer.BlendMode);
                    // draw while saving clip
                    if (needToSaveClip)
                    {
                        Chunk tempChunk = Chunk.Create(resolution);
                        // this chunk is empty
                        if (!layer.ReadOnlyLayerImage.DrawMostUpToDateChunkOn(chunkPos, resolution, tempChunk.Surface.SkiaSurface, new(0, 0), ReplacingPaint))
                        {
                            tempChunk.Dispose();
                            clippingChunk = new None();
                            continue;
                        }
                        // this chunk is not empty
                        layer.ReadOnlyMask.DrawMostUpToDateChunkOn(chunkPos, resolution, tempChunk.Surface.SkiaSurface, new(0, 0), ClippingPaint);
                        targetChunk.Surface.SkiaSurface.Canvas.DrawSurface(tempChunk.Surface.SkiaSurface, 0, 0, PaintToDrawChunksWith);
                        clippingChunk = tempChunk;
                    }
                    // draw using saved clip
                    else if (clipActiveWithReference)
                    {
                        if (clippingChunk.IsT1)
                            continue;
                        using Chunk tempChunk = Chunk.Create(resolution);
                        if (!layer.ReadOnlyLayerImage.DrawMostUpToDateChunkOn(chunkPos, resolution, tempChunk.Surface.SkiaSurface, new(0, 0), ReplacingPaint))
                            continue;
                        layer.ReadOnlyMask.DrawMostUpToDateChunkOn(chunkPos, resolution, tempChunk.Surface.SkiaSurface, new(0, 0), ClippingPaint);
                        OperationHelper.ClampAlpha(tempChunk.Surface.SkiaSurface, clippingChunk.AsT0.Surface.SkiaSurface);
                        targetChunk.Surface.SkiaSurface.Canvas.DrawSurface(tempChunk.Surface.SkiaSurface, 0, 0, PaintToDrawChunksWith);
                    }
                    // just draw
                    else
                    {
                        using Chunk tempChunk = Chunk.Create(resolution);
                        if (!layer.ReadOnlyLayerImage.DrawMostUpToDateChunkOn(chunkPos, resolution, tempChunk.Surface.SkiaSurface, new(0, 0), ReplacingPaint))
                            continue;
                        layer.ReadOnlyMask.DrawMostUpToDateChunkOn(chunkPos, resolution, tempChunk.Surface.SkiaSurface, new(0, 0), ClippingPaint);
                        targetChunk.Surface.SkiaSurface.Canvas.DrawSurface(tempChunk.Surface.SkiaSurface, 0, 0, PaintToDrawChunksWith);
                    }
                }
                continue;
            }

            // folder
            if (child is IReadOnlyFolder innerFolder)
            {
                PaintToDrawChunksWith.Color = new SKColor(255, 255, 255, (byte)Math.Round(child.Opacity * 255));
                PaintToDrawChunksWith.BlendMode = GetSKBlendMode(innerFolder.BlendMode);

                // draw while saving clip
                if (needToSaveClip)
                {
                    Chunk renderedChunk = RenderChunkRecursively(chunkPos, resolution, depth + 1, innerFolder, visibleLayers);
                    if (innerFolder.ReadOnlyMask is not null)
                        innerFolder.ReadOnlyMask.DrawMostUpToDateChunkOn(chunkPos, resolution, renderedChunk.Surface.SkiaSurface, new(0, 0), ClippingPaint);

                    renderedChunk.DrawOnSurface(targetChunk.Surface.SkiaSurface, new(0, 0), PaintToDrawChunksWith);
                    clippingChunk = renderedChunk;
                    continue;
                }
                // draw using saved clip
                else if (clipActiveWithReference)
                {
                    if (clippingChunk.IsT1)
                        continue;
                    using Chunk renderedChunk = RenderChunkRecursively(chunkPos, resolution, depth + 1, innerFolder, visibleLayers);
                    if (innerFolder.ReadOnlyMask is not null)
                        innerFolder.ReadOnlyMask.DrawMostUpToDateChunkOn(chunkPos, resolution, renderedChunk.Surface.SkiaSurface, new(0, 0), ClippingPaint);
                    OperationHelper.ClampAlpha(renderedChunk.Surface.SkiaSurface, clippingChunk.AsT0.Surface.SkiaSurface);
                    renderedChunk.DrawOnSurface(targetChunk.Surface.SkiaSurface, new(0, 0), PaintToDrawChunksWith);
                    continue;
                }
                // just draw
                else
                {
                    using Chunk renderedChunk = RenderChunkRecursively(chunkPos, resolution, depth + 1, innerFolder, visibleLayers);
                    if (innerFolder.ReadOnlyMask is not null)
                        innerFolder.ReadOnlyMask.DrawMostUpToDateChunkOn(chunkPos, resolution, renderedChunk.Surface.SkiaSurface, new(0, 0), ClippingPaint);
                    renderedChunk.DrawOnSurface(targetChunk.Surface.SkiaSurface, new(0, 0), PaintToDrawChunksWith);
                    continue;
                }
            }
        }
        if (clippingChunk.IsT0)
            clippingChunk.AsT0.Dispose();
        return targetChunk;
    }
}
