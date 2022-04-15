using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Rendering
{
    public static class ChunkRenderer
    {
        private static SKPaint PaintToDrawChunksWith = new SKPaint() { BlendMode = SKBlendMode.SrcOver };
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
                if (child is IReadOnlyLayer layer && (visibleLayers is null || visibleLayers.Contains(layer.GuidValue)))
                {
                    PaintToDrawChunksWith.Color = new SKColor(255, 255, 255, (byte)Math.Round(child.Opacity * 255));
                    layer.ReadOnlyLayerImage.DrawLatestChunkOn(chunkPos, resolution, targetChunk.Surface.SkiaSurface, new(0, 0), PaintToDrawChunksWith);
                }
                else if (child is IReadOnlyFolder innerFolder)
                {
                    using Chunk renderedChunk = RenderChunkRecursively(chunkPos, resolution, depth + 1, innerFolder, visibleLayers);
                    PaintToDrawChunksWith.Color = new SKColor(255, 255, 255, (byte)Math.Round(child.Opacity * 255));
                    renderedChunk.DrawOnSurface(targetChunk.Surface.SkiaSurface, new(0, 0), PaintToDrawChunksWith);
                }
            }
            return targetChunk;
        }
    }
}
