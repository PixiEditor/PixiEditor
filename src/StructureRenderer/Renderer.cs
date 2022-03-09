using ChangeableDocument;
using ChangeableDocument.Changeables.Interfaces;
using ChangeableDocument.ChangeInfos;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using SkiaSharp;
using StructureRenderer.RenderInfos;

namespace StructureRenderer
{
    public class Renderer
    {
        private DocumentChangeTracker tracker;
        private Surface? backSurface;
        private static SKPaint BlendingPaint = new SKPaint() { BlendMode = SKBlendMode.SrcOver };
        private static SKPaint ClearPaint = new SKPaint() { BlendMode = SKBlendMode.Src, Color = SKColors.Transparent };
        public Renderer(DocumentChangeTracker tracker)
        {
            this.tracker = tracker;
        }

        public async Task<List<IRenderInfo>> ProcessChanges(IReadOnlyList<IChangeInfo> changes, SKSurface screenSurface, int screenW, int screenH)
        {
            return await Task.Run(() => Render(changes, screenSurface, screenW, screenH)).ConfigureAwait(true);
        }

        private HashSet<Vector2i> FindChunksToRerender(IReadOnlyList<IChangeInfo> changes)
        {
            HashSet<Vector2i> chunks = new();
            foreach (var change in changes)
            {
                if (change is LayerImageChunks_ChangeInfo layerImageChunks)
                {
                    if (layerImageChunks.Chunks == null)
                        throw new Exception("Chunks must not be null");
                    chunks.UnionWith(layerImageChunks.Chunks);
                }
            }
            return chunks;
        }

        private List<IRenderInfo> Render(IReadOnlyList<IChangeInfo> changes, SKSurface screenSurface, int screenW, int screenH)
        {
            bool redrawEverything = false;
            if (backSurface == null || backSurface.Width != screenW || backSurface.Height != screenH)
            {
                backSurface?.Dispose();
                backSurface = new(screenW, screenH);
                redrawEverything = true;
            }

            DirtyRect_RenderInfo? info = null;
            // draw to back surface
            if (redrawEverything)
            {
                RenderScreen(screenW, screenH, screenSurface);
                info = new(0, 0, screenW, screenH);
            }
            else
            {
                HashSet<Vector2i> chunks = FindChunksToRerender(changes);
                var (minX, minY, maxX, maxY) = (int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);
                foreach (var chunkPos in chunks)
                {
                    RenderChunk(chunkPos, screenSurface);
                    (minX, minY) = (Math.Min(chunkPos.X, minX), Math.Min(chunkPos.Y, minY));
                    (maxX, maxY) = (Math.Max(chunkPos.X, maxX), Math.Max(chunkPos.Y, maxY));
                }
                if (minX != int.MaxValue)
                {
                    info = new(
                        minX * ChunkyImage.ChunkSize,
                        minY * ChunkyImage.ChunkSize,
                        (maxX - minX + 1) * ChunkyImage.ChunkSize,
                        (maxY - minY + 1) * ChunkyImage.ChunkSize);
                }
            }

            // transfer back surface to screen surface
            screenSurface.Canvas.DrawSurface(backSurface.SkiaSurface, 0, 0);

            return info == null ? new() : new() { info };
        }

        private void RenderScreen(int screenW, int screenH, SKSurface screenSurface)
        {
            int chunksWidth = (int)Math.Ceiling(screenW / (float)ChunkyImage.ChunkSize);
            int chunksHeight = (int)Math.Ceiling(screenH / (float)ChunkyImage.ChunkSize);
            for (int x = 0; x < chunksWidth; x++)
            {
                for (int y = 0; y < chunksHeight; y++)
                {
                    RenderChunk(new(x, y), screenSurface);
                }
            }
        }

        private void RenderChunk(Vector2i chunkPos, SKSurface screenSurface)
        {
            screenSurface.Canvas.DrawRect(SKRect.Create(chunkPos * ChunkyImage.ChunkSize, new(ChunkyImage.ChunkSize, ChunkyImage.ChunkSize)), ClearPaint);
            ForEachLayer((layer) =>
            {
                var chunk = layer.LayerImage.GetChunk(chunkPos);
                if (chunk == null)
                    return;
                using var snapshot = chunk.Snapshot();
                screenSurface.Canvas.DrawImage(snapshot, chunkPos * ChunkyImage.ChunkSize, BlendingPaint);
            }, tracker.Document.ReadOnlyStructureRoot);
        }

        private void ForEachLayer(Action<IReadOnlyLayer> action, IReadOnlyFolder folder)
        {
            foreach (var child in folder.ReadOnlyChildren)
            {
                if (child is IReadOnlyLayer layer)
                {
                    action(layer);
                }
                else if (child is IReadOnlyFolder innerFolder)
                {
                    ForEachLayer(action, innerFolder);
                }
            }
        }
    }
}
