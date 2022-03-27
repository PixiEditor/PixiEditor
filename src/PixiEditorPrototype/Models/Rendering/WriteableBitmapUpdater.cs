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
using System.Linq;
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

        private SKRect lastViewport = SKRect.Create(0, 0, 64, 64);

        private Dictionary<ChunkResolution, HashSet<Vector2i>> postponedChunks = new()
        {
            [ChunkResolution.Full] = new(),
            [ChunkResolution.Half] = new(),
            [ChunkResolution.Quarter] = new(),
            [ChunkResolution.Eighth] = new()
        };

        public WriteableBitmapUpdater(DocumentChangeTracker tracker)
        {
            this.tracker = tracker;
        }

        public async Task<List<IRenderInfo>> ProcessChanges(IReadOnlyList<IChangeInfo> changes, SKSurface screenSurface, ChunkResolution resolution)
        {
            return await Task.Run(() => Render(changes, screenSurface, resolution)).ConfigureAwait(true);
        }

        public List<IRenderInfo> ProcessChangesSync(IReadOnlyList<IChangeInfo> changes, SKSurface screenSurface, ChunkResolution resolution)
        {
            return Render(changes, screenSurface, resolution);
        }

        private HashSet<Vector2i> FindChunksToRerender(IReadOnlyList<IChangeInfo> changes, ChunkResolution resolution)
        {
            HashSet<Vector2i> affectedChunks = new();
            foreach (var change in changes)
            {
                switch (change)
                {
                    case LayerImageChunks_ChangeInfo layerImageChunks:
                        if (layerImageChunks.Chunks is null)
                            throw new InvalidOperationException("Chunks must not be null");
                        affectedChunks.UnionWith(layerImageChunks.Chunks);
                        break;
                    case Selection_ChangeInfo selection:
                        if (tracker.Document.ReadOnlySelection.ReadOnlyIsEmptyAndInactive)
                        {
                            AddAllChunks(affectedChunks);
                        }
                        else
                        {
                            if (selection.Chunks is null)
                                throw new InvalidOperationException("Chunks must not be null");
                            affectedChunks.UnionWith(selection.Chunks);
                        }
                        break;
                    case CreateStructureMember_ChangeInfo:
                    case DeleteStructureMember_ChangeInfo:
                    case MoveStructureMember_ChangeInfo:
                    case Size_ChangeInfo:
                        AddAllChunks(affectedChunks);
                        break;
                    case StructureMemberOpacity_ChangeInfo opacityChangeInfo:
                        var memberWithOpacity = tracker.Document.FindMemberOrThrow(opacityChangeInfo.GuidValue);
                        if (memberWithOpacity is IReadOnlyLayer layerWithOpacity)
                            affectedChunks.UnionWith(layerWithOpacity.ReadOnlyLayerImage.FindAllChunks());
                        else
                            AddAllChunks(affectedChunks);
                        break;
                    case StructureMemberIsVisible_ChangeInfo visibilityChangeInfo:
                        var memberWithVisibility = tracker.Document.FindMemberOrThrow(visibilityChangeInfo.GuidValue);
                        if (memberWithVisibility is IReadOnlyLayer layerWithVisibility)
                            affectedChunks.UnionWith(layerWithVisibility.ReadOnlyLayerImage.FindAllChunks());
                        else
                            AddAllChunks(affectedChunks);
                        break;
                    case MoveViewport_PassthroughAction moveViewportInfo:
                        lastViewport = moveViewportInfo.Viewport;
                        break;
                }
            }

            postponedChunks[ChunkResolution.Full].UnionWith(affectedChunks);
            postponedChunks[ChunkResolution.Half].UnionWith(affectedChunks);
            postponedChunks[ChunkResolution.Quarter].UnionWith(affectedChunks);
            postponedChunks[ChunkResolution.Eighth].UnionWith(affectedChunks);

            HashSet<Vector2i> visibleChunks = postponedChunks[resolution].Where(pos =>
            {
                var rect = SKRect.Create(pos, new(ChunkyImage.ChunkSize, ChunkyImage.ChunkSize));
                return rect.IntersectsWith(lastViewport);
            }).ToHashSet();
            postponedChunks[resolution].ExceptWith(visibleChunks);

            return visibleChunks;
        }

        private void AddAllChunks(HashSet<Vector2i> chunks)
        {
            Vector2i size = new(
                (int)Math.Ceiling(tracker.Document.Size.X / (float)ChunkyImage.ChunkSize),
                (int)Math.Ceiling(tracker.Document.Size.Y / (float)ChunkyImage.ChunkSize));
            for (int i = 0; i < size.X; i++)
            {
                for (int j = 0; j < size.Y; j++)
                {
                    chunks.Add(new(i, j));
                }
            }
        }

        private List<IRenderInfo> Render(IReadOnlyList<IChangeInfo> changes, SKSurface screenSurface, ChunkResolution resolution)
        {
            HashSet<Vector2i> chunks = FindChunksToRerender(changes, resolution);

            List<IRenderInfo> infos = new();

            int chunkSize = resolution.PixelSize();
            foreach (var chunkPos in chunks!)
            {
                RenderChunk(chunkPos, screenSurface, resolution);
                infos.Add(new DirtyRect_RenderInfo(
                    chunkPos * chunkSize,
                    new(chunkSize, chunkSize)
                    ));
            }

            return infos;
        }

        private void RenderChunk(Vector2i chunkPos, SKSurface screenSurface, ChunkResolution resolution)
        {
            using Chunk renderedChunk = ChunkRenderer.RenderWholeStructure(chunkPos, resolution, tracker.Document.ReadOnlyStructureRoot);

            screenSurface.Canvas.DrawSurface(renderedChunk.Surface.SkiaSurface, chunkPos.Multiply(renderedChunk.PixelSize), ReplacingPaint);

            if (tracker.Document.ReadOnlySelection.ReadOnlyIsEmptyAndInactive)
                return;
            IReadOnlyChunk? selectionChunk = tracker.Document.ReadOnlySelection.ReadOnlySelectionImage.GetLatestChunk(chunkPos, resolution);
            if (selectionChunk is not null)
                selectionChunk.DrawOnSurface(screenSurface, chunkPos.Multiply(selectionChunk.PixelSize), SelectionPaint);
        }
    }
}
