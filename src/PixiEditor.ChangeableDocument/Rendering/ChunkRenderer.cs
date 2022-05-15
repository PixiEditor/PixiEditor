using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
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
        Chunk targetChunk = Chunk.Create(resolution);
        targetChunk.Surface.SkiaSurface.Canvas.Clear();
        foreach (var child in folder.ReadOnlyChildren)
        {
            if (!child.IsVisible)
                continue;

            // chunk fully masked out
            if (child.ReadOnlyMask is not null && !child.ReadOnlyMask.LatestOrCommittedChunkExists(chunkPos))
                continue;

            // layer
            if (child is IReadOnlyLayer layer && (visibleLayers is null || visibleLayers.Contains(layer.GuidValue)))
            {
                if (layer.ReadOnlyMask is null)
                {
                    PaintToDrawChunksWith.Color = new SKColor(255, 255, 255, (byte)Math.Round(child.Opacity * 255));
                    PaintToDrawChunksWith.BlendMode = GetSKBlendMode(layer.BlendMode);
                    layer.ReadOnlyLayerImage.DrawMostUpToDateChunkOn(chunkPos, resolution, targetChunk.Surface.SkiaSurface, new(0, 0), PaintToDrawChunksWith);
                }
                else
                {
                    using (Chunk tempChunk = Chunk.Create(resolution))
                    {
                        if (!layer.ReadOnlyLayerImage.DrawMostUpToDateChunkOn(chunkPos, resolution, tempChunk.Surface.SkiaSurface, new(0, 0), ReplacingPaint))
                            continue;
                        layer.ReadOnlyMask.DrawMostUpToDateChunkOn(chunkPos, resolution, tempChunk.Surface.SkiaSurface, new(0, 0), ClippingPaint);

                        PaintToDrawChunksWith.Color = new SKColor(255, 255, 255, (byte)Math.Round(child.Opacity * 255));
                        PaintToDrawChunksWith.BlendMode = GetSKBlendMode(layer.BlendMode);
                        targetChunk.Surface.SkiaSurface.Canvas.DrawSurface(tempChunk.Surface.SkiaSurface, 0, 0, PaintToDrawChunksWith);
                    }
                }
                continue;
            }

            // folder
            if (child is IReadOnlyFolder innerFolder)
            {
                using Chunk renderedChunk = RenderChunkRecursively(chunkPos, resolution, depth + 1, innerFolder, visibleLayers);
                if (innerFolder.ReadOnlyMask is not null)
                    innerFolder.ReadOnlyMask.DrawMostUpToDateChunkOn(chunkPos, resolution, renderedChunk.Surface.SkiaSurface, new(0, 0), ClippingPaint);

                PaintToDrawChunksWith.Color = new SKColor(255, 255, 255, (byte)Math.Round(child.Opacity * 255));
                PaintToDrawChunksWith.BlendMode = GetSKBlendMode(innerFolder.BlendMode);
                renderedChunk.DrawOnSurface(targetChunk.Surface.SkiaSurface, new(0, 0), PaintToDrawChunksWith);
                continue;
            }
        }
        return targetChunk;
    }
}
