using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditorPrototype.Models.Rendering.RenderInfos;
using PixiEditorPrototype.ViewModels;
using SkiaSharp;

namespace PixiEditorPrototype.Models.Rendering
{
    internal class WriteableBitmapUpdater
    {
        private readonly DocumentViewModel doc;
        private readonly DocumentHelpers helpers;

        private static readonly SKPaint BlendingPaint = new SKPaint() { BlendMode = SKBlendMode.SrcOver };
        private static readonly SKPaint ReplacingPaint = new SKPaint() { BlendMode = SKBlendMode.Src };
        private static readonly SKPaint SelectionPaint = new SKPaint() { BlendMode = SKBlendMode.SrcOver, Color = new(0xa0FFFFFF) };
        private static readonly SKPaint ClearPaint = new SKPaint() { BlendMode = SKBlendMode.Src, Color = SKColors.Transparent };

        private readonly Dictionary<ChunkResolution, HashSet<Vector2i>> postponedChunks = new()
        {
            [ChunkResolution.Full] = new(),
            [ChunkResolution.Half] = new(),
            [ChunkResolution.Quarter] = new(),
            [ChunkResolution.Eighth] = new()
        };

        public WriteableBitmapUpdater(DocumentViewModel doc, DocumentHelpers helpers)
        {
            this.doc = doc;
            this.helpers = helpers;
        }

        public async Task<List<IRenderInfo>> ProcessChanges(IReadOnlyList<IChangeInfo?> changes)
        {
            return await Task.Run(() => Render(changes)).ConfigureAwait(true);
        }

        public List<IRenderInfo> ProcessChangesSync(IReadOnlyList<IChangeInfo?> changes)
        {
            return Render(changes);
        }

        private Dictionary<ChunkResolution, HashSet<Vector2i>> FindChunksToRerender(IReadOnlyList<IChangeInfo?> changes)
        {
            HashSet<Vector2i> affectedChunks = new();
            foreach (var change in changes)
            {
                switch (change)
                {
                    case MaskChunks_ChangeInfo maskChunks:
                        if (maskChunks.Chunks is null)
                            throw new InvalidOperationException("Chunks must not be null");
                        affectedChunks.UnionWith(maskChunks.Chunks);
                        break;
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
                    case StructureMemberMask_ChangeInfo:
                    case StructureMemberBlendMode_ChangeInfo:
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
                    case RefreshViewport_PassthroughAction moveViewportInfo:
                        break;
                }
            }

            foreach (var (_, postponed) in postponedChunks)
            {
                postponed.UnionWith(affectedChunks);
            }

            var chunksOnScreen = new Dictionary<ChunkResolution, HashSet<Vector2i>>()
            {
                [ChunkResolution.Full] = new(),
                [ChunkResolution.Half] = new(),
                [ChunkResolution.Quarter] = new(),
                [ChunkResolution.Eighth] = new()
            };

            foreach (var (_, viewport) in helpers.State.Viewports)
            {
                var viewportChunks = OperationHelper.FindChunksTouchingRectangle(
                    viewport.Center,
                    viewport.Dimensions,
                    -viewport.Angle,
                    ChunkResolution.Full.PixelSize());
                chunksOnScreen[viewport.Resolution].UnionWith(viewportChunks);
            }

            foreach (var (res, postponed) in postponedChunks)
            {
                chunksOnScreen[res].IntersectWith(postponed);
                postponed.ExceptWith(chunksOnScreen[res]);
            }

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

        private List<IRenderInfo> Render(IReadOnlyList<IChangeInfo?> changes)
        {
            Dictionary<ChunkResolution, HashSet<Vector2i>> chunksToRerender = FindChunksToRerender(changes);

            List<IRenderInfo> infos = new();

            foreach (var (resolution, chunks) in chunksToRerender)
            {
                int chunkSize = resolution.PixelSize();
                SKSurface screenSurface = doc.Surfaces[resolution];
                foreach (var chunkPos in chunks)
                {
                    RenderChunk(chunkPos, screenSurface, resolution);
                    infos.Add(new DirtyRect_RenderInfo(
                        chunkPos * chunkSize,
                        new(chunkSize, chunkSize),
                        resolution
                        ));
                }
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
