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
        private List<Surface> temporarySurfaces = new();
        private Surface? backSurface;
        private static SKPaint PaintToDrawChunksWith = new SKPaint() { BlendMode = SKBlendMode.SrcOver };
        private static SKPaint BlendingPaint = new SKPaint() { BlendMode = SKBlendMode.SrcOver };
        private static SKPaint ReplacingPaint = new SKPaint() { BlendMode = SKBlendMode.Src };
        private static SKPaint SelectionPaint = new SKPaint() { BlendMode = SKBlendMode.SrcOver, Color = new(0xa0FFFFFF) };
        private static SKPaint ClearPaint = new SKPaint() { BlendMode = SKBlendMode.Src, Color = SKColors.Transparent };

        private bool highlightUpdatedChunks = false;
        private SKPaint highlightPaint = new SKPaint() { BlendMode = SKBlendMode.SrcOver, Color = new(0x30FF0000) };

        public Renderer(DocumentChangeTracker tracker)
        {
            this.tracker = tracker;
        }

        public async Task<List<IRenderInfo>> ProcessChanges(IReadOnlyList<IChangeInfo> changes, SKSurface screenSurface, Vector2i screenSize)
        {
            return await Task.Run(() => Render(changes, screenSurface, screenSize)).ConfigureAwait(true);
        }

        private HashSet<Vector2i>? FindChunksToRerender(IReadOnlyList<IChangeInfo> changes)
        {
            HashSet<Vector2i> chunks = new();
            foreach (var change in changes)
            {
                switch (change)
                {
                    case LayerImageChunks_ChangeInfo layerImageChunks:
                        if (layerImageChunks.Chunks == null)
                            throw new Exception("Chunks must not be null");
                        chunks.UnionWith(layerImageChunks.Chunks);
                        break;
                    case Selection_ChangeInfo selection:
                        if (tracker.Document.ReadOnlySelection.ReadOnlyIsEmptyAndInactive)
                        {
                            return null;
                        }
                        else
                        {
                            if (selection.Chunks == null)
                                throw new Exception("Chunks must not be null");
                            chunks.UnionWith(selection.Chunks);
                        }
                        break;
                    case CreateStructureMember_ChangeInfo:
                    case DeleteStructureMember_ChangeInfo:
                    case MoveStructureMember_ChangeInfo:
                    case Size_ChangeInfo:
                        return null;
                    case StructureMemberOpacity_ChangeInfo opacityChangeInfo:
                        var memberWithOpacity = tracker.Document.FindMemberOrThrow(opacityChangeInfo.GuidValue);
                        if (memberWithOpacity is IReadOnlyLayer layerWithOpacity)
                            chunks.UnionWith(layerWithOpacity.ReadOnlyLayerImage.FindAllChunks());
                        else
                            return null;
                        break;
                    case StructureMemberProperties_ChangeInfo propertiesChangeInfo:
                        if (!propertiesChangeInfo.IsVisibleChanged)
                            break;
                        var memberWithVisibility = tracker.Document.FindMemberOrThrow(propertiesChangeInfo.GuidValue);
                        if (memberWithVisibility is IReadOnlyLayer layerWithVisibility)
                            chunks.UnionWith(layerWithVisibility.ReadOnlyLayerImage.FindAllChunks());
                        else
                            return null;
                        break;
                }
            }
            return chunks;
        }

        private List<IRenderInfo> Render(IReadOnlyList<IChangeInfo> changes, SKSurface screenSurface, Vector2i screenSize)
        {
            bool redrawEverything = false;
            if (backSurface == null || backSurface.Size != screenSize)
            {
                backSurface?.Dispose();
                backSurface = new(screenSize);
                redrawEverything = true;
            }
            HashSet<Vector2i>? chunks = null;
            if (!redrawEverything)
                chunks = FindChunksToRerender(changes);
            if (chunks == null)
                redrawEverything = true;

            AllocateTempSurfaces(tracker.Document.ReadOnlyStructureRoot);

            List<IRenderInfo> infos = new();

            // draw to back surface
            if (redrawEverything)
            {
                RenderScreen(screenSize, backSurface.SkiaSurface, tracker.Document.ReadOnlyStructureRoot);
                infos.Add(new DirtyRect_RenderInfo(new Vector2i(0, 0), screenSize));
            }
            else
            {
                if (highlightUpdatedChunks)
                {
                    backSurface.SkiaSurface.Canvas.Clear();
                    infos.Add(new DirtyRect_RenderInfo(new(0, 0), screenSize));
                }
                foreach (var chunkPos in chunks!)
                {
                    backSurface.SkiaSurface.Canvas.DrawRect(SKRect.Create(chunkPos * ChunkyImage.ChunkSize, new(ChunkyImage.ChunkSize, ChunkyImage.ChunkSize)), ClearPaint);
                    RenderChunk(chunkPos, backSurface.SkiaSurface);
                    infos.Add(new DirtyRect_RenderInfo(
                        chunkPos * ChunkyImage.ChunkSize,
                        new(ChunkyImage.ChunkSize, ChunkyImage.ChunkSize)
                        ));
                }
            }

            // transfer the back surface to the screen surface

            foreach (var info in infos)
            {
                if (info is DirtyRect_RenderInfo dirtyRect)
                {
                    screenSurface.Canvas.Save();
                    screenSurface.Canvas.ClipRect(SKRect.Create(dirtyRect.Pos, dirtyRect.Size));
                    screenSurface.Canvas.DrawSurface(backSurface.SkiaSurface, 0, 0, ReplacingPaint);
                    screenSurface.Canvas.Restore();
                }
            }

            return infos;
        }

        private void RenderScreen(Vector2i screenSize, SKSurface screenSurface, IReadOnlyFolder structureRoot)
        {
            int chunksWidth = (int)Math.Ceiling(screenSize.X / (float)ChunkyImage.ChunkSize);
            int chunksHeight = (int)Math.Ceiling(screenSize.Y / (float)ChunkyImage.ChunkSize);
            screenSurface.Canvas.Clear();
            for (int x = 0; x < chunksWidth; x++)
            {
                for (int y = 0; y < chunksHeight; y++)
                {
                    RenderChunk(new(x, y), screenSurface);
                }
            }
        }

        private void AllocateTempSurfaces(IReadOnlyFolder structureRoot)
        {
            int depth = FindDeepestLayerDepth(structureRoot, 0);
            while (temporarySurfaces.Count < depth)
            {
                temporarySurfaces.Add(new Surface(new Vector2i(ChunkyImage.ChunkSize, ChunkyImage.ChunkSize)));
            }
        }

        private int FindDeepestLayerDepth(IReadOnlyFolder folder, int folderDepth)
        {
            int deepestLayer = -1;
            foreach (var child in folder.ReadOnlyChildren)
            {
                if (child is IReadOnlyLayer layer)
                {
                    deepestLayer = folderDepth + 1;
                }
                else if (child is IReadOnlyFolder innerFolder)
                {
                    deepestLayer = FindDeepestLayerDepth(innerFolder, folderDepth + 1);
                }
            }
            return deepestLayer;
        }

        private void RenderChunk(Vector2i chunkPos, SKSurface screenSurface)
        {
            var renderedSurface = RenderChunkRecursively(chunkPos, 0, tracker.Document.ReadOnlyStructureRoot);
            if (renderedSurface != null)
            {
                if (highlightUpdatedChunks)
                    renderedSurface.SkiaSurface.Canvas.DrawPaint(highlightPaint);
                screenSurface.Canvas.DrawSurface(renderedSurface.SkiaSurface, chunkPos * ChunkyImage.ChunkSize, BlendingPaint);
            }

            if (tracker.Document.ReadOnlySelection.ReadOnlyIsEmptyAndInactive)
                return;
            var selectionChunk = tracker.Document.ReadOnlySelection.ReadOnlySelectionImage.GetLatestChunk(chunkPos);
            if (selectionChunk != null)
                selectionChunk.DrawOnSurface(screenSurface, chunkPos * ChunkyImage.ChunkSize, SelectionPaint);

        }

        private Surface? RenderChunkRecursively(Vector2i chunkPos, int depth, IReadOnlyFolder folder)
        {
            Surface? surface = temporarySurfaces.Count > depth ? temporarySurfaces[depth] : null;
            surface?.SkiaSurface.Canvas.Clear();
            foreach (var child in folder.ReadOnlyChildren)
            {
                if (!child.IsVisible)
                    continue;
                if (child is IReadOnlyLayer layer)
                {
                    var chunk = layer.ReadOnlyLayerImage.GetLatestChunk(chunkPos);
                    if (chunk == null)
                        continue;
                    if (surface == null)
                        throw new Exception("Not enough surfaces have been allocated to draw the entire layer tree");
                    PaintToDrawChunksWith.Color = new SKColor(255, 255, 255, (byte)Math.Round(child.Opacity * 255));
                    chunk.DrawOnSurface(surface.SkiaSurface, new(0, 0), PaintToDrawChunksWith);
                }
                else if (child is IReadOnlyFolder innerFolder)
                {
                    var renderedSurface = RenderChunkRecursively(chunkPos, depth + 1, innerFolder);
                    if (renderedSurface == null)
                        continue;
                    if (surface == null)
                        throw new Exception("Not enough surfaces have been allocated to draw the entire layer tree");
                    PaintToDrawChunksWith.Color = new SKColor(255, 255, 255, (byte)Math.Round(child.Opacity * 255));
                    surface.SkiaSurface.Canvas.DrawSurface(renderedSurface.SkiaSurface, 0, 0, PaintToDrawChunksWith);
                }
            }
            return surface;
        }
    }
}
