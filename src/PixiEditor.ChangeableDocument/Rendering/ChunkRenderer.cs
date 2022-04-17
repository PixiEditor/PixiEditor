using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Rendering
{
    public static class ChunkRenderer
    {
        private static SKPaint PaintToDrawChunksWith = new SKPaint() { BlendMode = SKBlendMode.SrcOver };
        private static SKPaint ReplacingPaint = new SKPaint() { BlendMode = SKBlendMode.Src };
        private static SKPaint ClippingPaint = new SKPaint() { BlendMode = SKBlendMode.DstIn };
        public static Chunk RenderWholeStructure(Vector2i pos, ChunkResolution resolution, IReadOnlyFolder root)
        {
            return RenderChunkRecursively(pos, resolution, 0, root, null);
        }

        public static Chunk RenderSpecificLayers(Vector2i pos, ChunkResolution resolution, IReadOnlyFolder root, HashSet<Guid> layers)
        {
            return RenderChunkRecursively(pos, resolution, 0, root, layers);
        }

        private static Chunk RenderChunkRecursively(Vector2i chunkPos, ChunkResolution resolution, int depth, IReadOnlyFolder folder, HashSet<Guid>? visibleLayers)
        {
            Chunk targetChunk = Chunk.Create(resolution);
            targetChunk.Surface.SkiaSurface.Canvas.Clear();
            foreach (var child in folder.ReadOnlyChildren)
            {
                if (!child.IsVisible)
                    continue;

                // chunk fully masked out
                if (child.ReadOnlyMask is not null && !child.ReadOnlyMask.LatestChunkExists(chunkPos, resolution))
                    continue;

                // layer
                if (child is IReadOnlyLayer layer && (visibleLayers is null || visibleLayers.Contains(layer.GuidValue)))
                {
                    if (layer.ReadOnlyMask is null)
                    {
                        PaintToDrawChunksWith.Color = new SKColor(255, 255, 255, (byte)Math.Round(child.Opacity * 255));
                        layer.ReadOnlyLayerImage.DrawLatestChunkOn(chunkPos, resolution, targetChunk.Surface.SkiaSurface, new(0, 0), PaintToDrawChunksWith);
                    }
                    else
                    {
                        using (Chunk tempChunk = Chunk.Create(resolution))
                        {
                            if (!layer.ReadOnlyLayerImage.DrawLatestChunkOn(chunkPos, resolution, tempChunk.Surface.SkiaSurface, new(0, 0), ReplacingPaint))
                                continue;
                            layer.ReadOnlyMask.DrawLatestChunkOn(chunkPos, resolution, tempChunk.Surface.SkiaSurface, new(0, 0), ClippingPaint);

                            PaintToDrawChunksWith.Color = new SKColor(255, 255, 255, (byte)Math.Round(child.Opacity * 255));
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
                        innerFolder.ReadOnlyMask.DrawLatestChunkOn(chunkPos, resolution, renderedChunk.Surface.SkiaSurface, new(0, 0), ClippingPaint);

                    PaintToDrawChunksWith.Color = new SKColor(255, 255, 255, (byte)Math.Round(child.Opacity * 255));
                    renderedChunk.DrawOnSurface(targetChunk.Surface.SkiaSurface, new(0, 0), PaintToDrawChunksWith);
                    continue;
                }
            }
            return targetChunk;
        }
    }
}
