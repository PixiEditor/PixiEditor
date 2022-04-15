using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditorPrototype.Models.Rendering.RenderInfos;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PixiEditorPrototype.Models.Rendering
{
    internal class WriteableBitmapUpdater
    {
        private DocumentHelpers helpers;

        private static SKPaint BlendingPaint = new SKPaint() { BlendMode = SKBlendMode.SrcOver };
        private static SKPaint ReplacingPaint = new SKPaint() { BlendMode = SKBlendMode.Src };
        private static SKPaint SelectionPaint = new SKPaint() { BlendMode = SKBlendMode.SrcOver, Color = new(0xa0FFFFFF) };
        private static SKPaint ClearPaint = new SKPaint() { BlendMode = SKBlendMode.Src, Color = SKColors.Transparent };

        private Dictionary<ChunkResolution, HashSet<Vector2i>> postponedChunks = new()
        {
            [ChunkResolution.Full] = new(),
            [ChunkResolution.Half] = new(),
            [ChunkResolution.Quarter] = new(),
            [ChunkResolution.Eighth] = new()
        };

        public WriteableBitmapUpdater(DocumentHelpers helpers)
        {
            this.helpers = helpers;
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
                        if (helpers.Tracker.Document.ReadOnlySelection.ReadOnlyIsEmptyAndInactive)
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
                        var memberWithOpacity = helpers.Tracker.Document.FindMemberOrThrow(opacityChangeInfo.GuidValue);
                        if (memberWithOpacity is IReadOnlyLayer layerWithOpacity)
                            affectedChunks.UnionWith(layerWithOpacity.ReadOnlyLayerImage.FindAllChunks());
                        else
                            AddAllChunks(affectedChunks);
                        break;
                    case StructureMemberIsVisible_ChangeInfo visibilityChangeInfo:
                        var memberWithVisibility = helpers.Tracker.Document.FindMemberOrThrow(visibilityChangeInfo.GuidValue);
                        if (memberWithVisibility is IReadOnlyLayer layerWithVisibility)
                            affectedChunks.UnionWith(layerWithVisibility.ReadOnlyLayerImage.FindAllChunks());
                        else
                            AddAllChunks(affectedChunks);
                        break;
                    case MoveViewport_PassthroughAction moveViewportInfo:

                        break;
                }
            }

            postponedChunks[ChunkResolution.Full].UnionWith(affectedChunks);
            postponedChunks[ChunkResolution.Half].UnionWith(affectedChunks);
            postponedChunks[ChunkResolution.Quarter].UnionWith(affectedChunks);
            postponedChunks[ChunkResolution.Eighth].UnionWith(affectedChunks);

            var chunksOnScreen = OperationHelper.FindChunksTouchingRectangle(
                helpers.State.ViewportCenter,
                helpers.State.ViewportSize,
                -helpers.State.ViewportAngle,
                ChunkResolution.Full.PixelSize());

            chunksOnScreen.IntersectWith(postponedChunks[resolution]);
            postponedChunks[resolution].ExceptWith(chunksOnScreen);

            return chunksOnScreen;
        }

        private void AddAllChunks(HashSet<Vector2i> chunks)
        {
            Vector2i size = new(
                (int)Math.Ceiling(helpers.Tracker.Document.Size.X / (float)ChunkyImage.ChunkSize),
                (int)Math.Ceiling(helpers.Tracker.Document.Size.Y / (float)ChunkyImage.ChunkSize));
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
            using Chunk renderedChunk = ChunkRenderer.RenderWholeStructure(chunkPos, resolution, helpers.Tracker.Document.ReadOnlyStructureRoot);

            screenSurface.Canvas.DrawSurface(renderedChunk.Surface.SkiaSurface, chunkPos.Multiply(renderedChunk.PixelSize), ReplacingPaint);

            if (helpers.Tracker.Document.ReadOnlySelection.ReadOnlyIsEmptyAndInactive)
                return;

            helpers.Tracker.Document.ReadOnlySelection.ReadOnlySelectionImage.DrawLatestChunkOn(chunkPos, resolution, screenSurface, chunkPos * resolution.PixelSize(), SelectionPaint);
        }
    }
}
