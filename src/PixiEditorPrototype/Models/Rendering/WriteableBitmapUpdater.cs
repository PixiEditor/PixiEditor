using ChangeableDocument;
using ChangeableDocument.Changeables.Interfaces;
using ChangeableDocument.ChangeInfos;
using ChangeableDocument.Rendering;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditorPrototype.Models.Rendering.RenderInfos;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PixiEditorPrototype.Models.Rendering
{
    public class WriteableBitmapUpdater
    {
        private DocumentChangeTracker tracker;
        private static SKPaint BlendingPaint = new SKPaint() { BlendMode = SKBlendMode.SrcOver };
        private static SKPaint ReplacingPaint = new SKPaint() { BlendMode = SKBlendMode.Src };
        private static SKPaint SelectionPaint = new SKPaint() { BlendMode = SKBlendMode.SrcOver, Color = new(0xa0FFFFFF) };
        private static SKPaint ClearPaint = new SKPaint() { BlendMode = SKBlendMode.Src, Color = SKColors.Transparent };
        private Vector2i oldSize = new(0, 0);

        public WriteableBitmapUpdater(DocumentChangeTracker tracker)
        {
            this.tracker = tracker;
        }

        public async Task<List<IRenderInfo>> ProcessChanges(IReadOnlyList<IChangeInfo> changes, SKSurface screenSurface, Vector2i screenSize)
        {
            return await Task.Run(() => Render(changes, screenSurface, screenSize)).ConfigureAwait(true);
        }

        public List<IRenderInfo> ProcessChangesSync(IReadOnlyList<IChangeInfo> changes, SKSurface screenSurface, Vector2i screenSize)
        {
            return Render(changes, screenSurface, screenSize);
        }

        private HashSet<Vector2i>? FindChunksToRerender(IReadOnlyList<IChangeInfo> changes)
        {
            HashSet<Vector2i> chunks = new();
            foreach (var change in changes)
            {
                switch (change)
                {
                    case LayerImageChunks_ChangeInfo layerImageChunks:
                        if (layerImageChunks.Chunks is null)
                            throw new InvalidOperationException("Chunks must not be null");
                        chunks.UnionWith(layerImageChunks.Chunks);
                        break;
                    case Selection_ChangeInfo selection:
                        if (tracker.Document.ReadOnlySelection.ReadOnlyIsEmptyAndInactive)
                        {
                            return null;
                        }
                        else
                        {
                            if (selection.Chunks is null)
                                throw new InvalidOperationException("Chunks must not be null");
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
                    case StructureMemberIsVisible_ChangeInfo visibilityChangeInfo:
                        var memberWithVisibility = tracker.Document.FindMemberOrThrow(visibilityChangeInfo.GuidValue);
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
            if (oldSize != screenSize)
            {
                oldSize = screenSize;
                redrawEverything = true;
            }
            HashSet<Vector2i>? chunks = null;
            if (!redrawEverything)
                chunks = FindChunksToRerender(changes);
            if (chunks is null)
                redrawEverything = true;


            List<IRenderInfo> infos = new();

            // draw to back surface
            if (redrawEverything)
            {
                RenderScreen(screenSize, screenSurface);
                infos.Add(new DirtyRect_RenderInfo(new Vector2i(0, 0), screenSize));
            }
            else
            {
                foreach (var chunkPos in chunks!)
                {
                    RenderChunk(chunkPos, screenSurface);
                    infos.Add(new DirtyRect_RenderInfo(
                        chunkPos * ChunkyImage.ChunkSize,
                        new(ChunkyImage.ChunkSize, ChunkyImage.ChunkSize)
                        ));
                }
            }

            return infos;
        }

        private void RenderScreen(Vector2i screenSize, SKSurface screenSurface)
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

        private void RenderChunk(Vector2i chunkPos, SKSurface screenSurface)
        {
            using Chunk renderedChunk = ChunkRenderer.RenderWholeStructure(chunkPos, tracker.Document.ReadOnlyStructureRoot);
            screenSurface.Canvas.DrawSurface(renderedChunk.Surface.SkiaSurface, chunkPos * ChunkyImage.ChunkSize, ReplacingPaint);

            if (tracker.Document.ReadOnlySelection.ReadOnlyIsEmptyAndInactive)
                return;
            IReadOnlyChunk? selectionChunk = tracker.Document.ReadOnlySelection.ReadOnlySelectionImage.GetLatestChunk(chunkPos);
            if (selectionChunk is not null)
                selectionChunk.DrawOnSurface(screenSurface, chunkPos * ChunkyImage.ChunkSize, SelectionPaint);
        }
    }
}
