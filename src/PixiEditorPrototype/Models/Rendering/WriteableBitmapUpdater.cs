using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using OneOf;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditorPrototype.Models.Rendering.RenderInfos;
using PixiEditorPrototype.ViewModels;
using SkiaSharp;

namespace PixiEditorPrototype.Models.Rendering;

internal class WriteableBitmapUpdater
{
    private readonly DocumentViewModel doc;
    private readonly DocumentHelpers helpers;

    private static readonly SKPaint BlendingPaint = new SKPaint() { BlendMode = SKBlendMode.SrcOver };
    private static readonly SKPaint ReplacingPaint = new SKPaint() { BlendMode = SKBlendMode.Src };
    private static readonly SKPaint SelectionPaint = new SKPaint() { BlendMode = SKBlendMode.SrcOver, Color = new(0xa0FFFFFF) };
    private static readonly SKPaint ClearPaint = new SKPaint() { BlendMode = SKBlendMode.Src, Color = SKColors.Transparent };

    private readonly Dictionary<ChunkResolution, HashSet<VecI>> globalPostponedChunks = new()
    {
        [ChunkResolution.Full] = new(),
        [ChunkResolution.Half] = new(),
        [ChunkResolution.Quarter] = new(),
        [ChunkResolution.Eighth] = new()
    };

    private Dictionary<Guid, HashSet<VecI>> previewPostponedChunks = new();
    private Dictionary<Guid, HashSet<VecI>> maskPostponedChunks = new();

    public WriteableBitmapUpdater(DocumentViewModel doc, DocumentHelpers helpers)
    {
        this.doc = doc;
        this.helpers = helpers;
    }

    public async Task<List<IRenderInfo>> UpdateGatheredChunks(AffectedChunkGatherer chunkGatherer, bool updatePreviews)
    {
        return await Task.Run(() => Render(chunkGatherer, updatePreviews)).ConfigureAwait(true);
    }

    private Dictionary<ChunkResolution, HashSet<VecI>> FindGlobalChunksToRerender(AffectedChunkGatherer chunkGatherer)
    {
        foreach (var (_, postponed) in globalPostponedChunks)
        {
            postponed.UnionWith(chunkGatherer.mainImageChunks);
        }

        var chunksOnScreen = new Dictionary<ChunkResolution, HashSet<VecI>>()
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

        foreach (var (res, postponed) in globalPostponedChunks)
        {
            chunksOnScreen[res].IntersectWith(postponed);
            postponed.ExceptWith(chunksOnScreen[res]);
        }

        return chunksOnScreen;
    }


    private static void AddChunks(Dictionary<Guid, HashSet<VecI>> from, Dictionary<Guid, HashSet<VecI>> to)
    {
        foreach ((Guid guid, HashSet<VecI> chunks) in from)
        {
            if (!to.ContainsKey(guid))
                to[guid] = new HashSet<VecI>();
            to[guid].UnionWith(chunks);
        }
    }
    private (Dictionary<Guid, HashSet<VecI>> image, Dictionary<Guid, HashSet<VecI>> mask) FindPreviewChunksToRerender
        (AffectedChunkGatherer chunkGatherer, bool postpone)
    {
        AddChunks(chunkGatherer.imagePreviewChunks, previewPostponedChunks);
        AddChunks(chunkGatherer.maskPreviewChunks, maskPostponedChunks);
        if (postpone)
            return (new(), new());
        var result = (previewPostponedChunks, maskPostponedChunks);
        previewPostponedChunks = new();
        maskPostponedChunks = new();
        return result;
    }

    private List<IRenderInfo> Render(AffectedChunkGatherer chunkGatherer, bool updatePreviews)
    {
        Dictionary<ChunkResolution, HashSet<VecI>> chunksToRerender = FindGlobalChunksToRerender(chunkGatherer);

        List<IRenderInfo> infos = new();
        UpdateMainImage(chunksToRerender, infos);

        var (imagePreviewChunksToRerender, maskPreviewChunksToRerender) = FindPreviewChunksToRerender(chunkGatherer, !updatePreviews);
        var previewSize = StructureMemberViewModel.CalculatePreviewSize(helpers.Tracker.Document.Size);
        float scaling = (float)previewSize.X / doc.SizeBindable.X;
        UpdateImagePreviews(imagePreviewChunksToRerender, scaling, infos);
        UpdateMaskPreviews(maskPreviewChunksToRerender, scaling, infos);

        return infos;
    }

    private void UpdateImagePreviews(Dictionary<Guid, HashSet<VecI>> imagePreviewChunks, float scaling, List<IRenderInfo> infos)
    {
        foreach (var (guid, chunks) in imagePreviewChunks)
        {
            var memberVM = helpers.StructureHelper.Find(guid);
            if (memberVM is null)
                continue;
            var member = helpers.Tracker.Document.FindMemberOrThrow(guid);

            memberVM.PreviewSurface.Canvas.Save();
            memberVM.PreviewSurface.Canvas.Scale(scaling);
            if (memberVM is LayerViewModel)
            {
                var layer = (IReadOnlyLayer)member;
                foreach (var chunk in chunks)
                {
                    var pos = chunk * ChunkResolution.Full.PixelSize();
                    // the full res chunks are already rendered so drawing them again should be fast
                    if (!layer.LayerImage.DrawMostUpToDateChunkOn
                        (chunk, ChunkResolution.Full, memberVM.PreviewSurface, pos, ReplacingPaint))
                        memberVM.PreviewSurface.Canvas.DrawRect(pos.X, pos.Y, ChunkyImage.FullChunkSize, ChunkyImage.FullChunkSize, ClearPaint);
                }
                infos.Add(new PreviewDirty_RenderInfo(guid));
            }
            else if (memberVM is FolderViewModel)
            {
                var folder = (IReadOnlyFolder)member;
                foreach (var chunk in chunks)
                {
                    var pos = chunk * ChunkResolution.Full.PixelSize();
                    // drawing in full res here is kinda slow
                    // we could switch to a lower resolution based on (canvas size / preview size) to make it run faster
                    OneOf<Chunk, EmptyChunk> rendered = ChunkRenderer.MergeWholeStructure(chunk, ChunkResolution.Full, folder);
                    if (rendered.IsT0)
                    {
                        memberVM.PreviewSurface.Canvas.DrawSurface(rendered.AsT0.Surface.SkiaSurface, pos, ReplacingPaint);
                        rendered.AsT0.Dispose();
                    }
                    else
                    {
                        memberVM.PreviewSurface.Canvas.DrawRect(pos.X, pos.Y, ChunkResolution.Full.PixelSize(), ChunkResolution.Full.PixelSize(), ClearPaint);
                    }
                }
                infos.Add(new PreviewDirty_RenderInfo(guid));
            }
            memberVM.PreviewSurface.Canvas.Restore();
        }
    }

    private void UpdateMaskPreviews(Dictionary<Guid, HashSet<VecI>> maskPreviewChunks, float scaling, List<IRenderInfo> infos)
    {
        foreach (var (guid, chunks) in maskPreviewChunks)
        {
            var memberVM = helpers.StructureHelper.Find(guid);
            if (memberVM is null || !memberVM.HasMaskBindable)
                continue;

            var member = helpers.Tracker.Document.FindMemberOrThrow(guid);
            memberVM.MaskPreviewSurface!.Canvas.Save();
            memberVM.MaskPreviewSurface.Canvas.Scale(scaling);

            foreach (var chunk in chunks)
            {
                var pos = chunk * ChunkResolution.Full.PixelSize();
                member.Mask!.DrawMostUpToDateChunkOn
                    (chunk, ChunkResolution.Full, memberVM.MaskPreviewSurface, pos, ReplacingPaint);
            }

            memberVM.MaskPreviewSurface.Canvas.Restore();
            infos.Add(new MaskPreviewDirty_RenderInfo(guid));
        }
    }

    private void UpdateMainImage(Dictionary<ChunkResolution, HashSet<VecI>> chunksToRerender, List<IRenderInfo> infos)
    {
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
    }

    private void RenderChunk(VecI chunkPos, SKSurface screenSurface, ChunkResolution resolution)
    {
        ChunkRenderer.MergeWholeStructure(chunkPos, resolution, helpers.Tracker.Document.StructureRoot).Switch(
            (Chunk chunk) =>
            {
                screenSurface.Canvas.DrawSurface(chunk.Surface.SkiaSurface, chunkPos.Multiply(chunk.PixelSize), ReplacingPaint);
                chunk.Dispose();
            },
            (EmptyChunk chunk) =>
            {
                var pos = chunkPos * resolution.PixelSize();
                screenSurface.Canvas.DrawRect(pos.X, pos.Y, resolution.PixelSize(), resolution.PixelSize(), ClearPaint);
            });
    }
}
