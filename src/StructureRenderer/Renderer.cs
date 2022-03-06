using ChangeableDocument;
using ChangeableDocument.Changeables.Interfaces;
using ChangeableDocument.ChangeInfos;
using ChunkyImageLib;
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

        private HashSet<(int, int)> FindChunksToRerender(IReadOnlyList<IChangeInfo> changes)
        {
            HashSet<(int, int)> chunks = new();
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
                HashSet<(int, int)> chunks = FindChunksToRerender(changes);
                var (minX, minY, maxX, maxY) = (int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);
                foreach (var (x, y) in chunks)
                {
                    RenderChunk(x, y, screenSurface);
                    (minX, minY) = (Math.Min(x, minX), Math.Min(y, minY));
                    (maxX, maxY) = (Math.Max(x, maxX), Math.Max(y, maxY));
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
                    RenderChunk(x, y, screenSurface);
                }
            }
        }

        private void RenderChunk(int chunkX, int chunkY, SKSurface screenSurface)
        {
            screenSurface.Canvas.DrawRect(chunkX * ChunkyImage.ChunkSize, chunkY * ChunkyImage.ChunkSize, ChunkyImage.ChunkSize, ChunkyImage.ChunkSize, ClearPaint);
            ForEachLayer((layer) =>
            {
                var chunk = layer.LayerImage.GetChunk(chunkX, chunkY);
                if (chunk == null)
                    return;
                using var snapshot = chunk.Snapshot();
                screenSurface.Canvas.DrawImage(snapshot, chunkX * ChunkyImage.ChunkSize, chunkY * ChunkyImage.ChunkSize, BlendingPaint);
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
