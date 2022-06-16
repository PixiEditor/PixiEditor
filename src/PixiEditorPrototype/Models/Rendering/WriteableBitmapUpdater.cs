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

    private static readonly SKPaint ReplacingPaint = new SKPaint() { BlendMode = SKBlendMode.Src };
    private static readonly SKPaint SmoothReplacingPaint = new SKPaint() { BlendMode = SKBlendMode.Src, FilterQuality = SKFilterQuality.Medium, IsAntialias = true };
    private static readonly SKPaint ClearPaint = new SKPaint() { BlendMode = SKBlendMode.Src, Color = SKColors.Transparent };

    private readonly Dictionary<ChunkResolution, HashSet<VecI>> globalPostponedChunks = new()
    {
        [ChunkResolution.Full] = new(),
        [ChunkResolution.Half] = new(),
        [ChunkResolution.Quarter] = new(),
        [ChunkResolution.Eighth] = new()
    };

    private readonly Dictionary<ChunkResolution, HashSet<VecI>> globalPostponedForDelayed = new()
    {
        [ChunkResolution.Full] = new(),
        [ChunkResolution.Half] = new(),
        [ChunkResolution.Quarter] = new(),
        [ChunkResolution.Eighth] = new()
    };
    
    private readonly Dictionary<ChunkResolution, HashSet<VecI>> globalDelayedChunks = new()
    {
        [ChunkResolution.Full] = new(),
        [ChunkResolution.Half] = new(),
        [ChunkResolution.Quarter] = new(),
        [ChunkResolution.Eighth] = new()
    };
    
    private Dictionary<Guid, HashSet<VecI>> previewDelayedChunks = new();
    private Dictionary<Guid, HashSet<VecI>> maskPreviewDelayedChunks = new();

    public WriteableBitmapUpdater(DocumentViewModel doc, DocumentHelpers helpers)
    {
        this.doc = doc;
        this.helpers = helpers;
    }

    /// <summary>
    /// Don't call this outside ActionAccumulator
    /// </summary>
    public async Task<List<IRenderInfo>> UpdateGatheredChunks
        (AffectedChunkGatherer chunkGatherer, bool updateDelayed)
    {
        return await Task.Run(() => Render(chunkGatherer, updateDelayed)).ConfigureAwait(true);
    }

    private Dictionary<ChunkResolution, HashSet<VecI>> FindGlobalChunksToRerender(AffectedChunkGatherer chunkGatherer, bool renderDelayed)
    {
        // add all affected chunks to postponed
        foreach (var (_, postponed) in globalPostponedChunks)
        {
            postponed.UnionWith(chunkGatherer.mainImageChunks);
        }

        // find all chunks that are on viewports and on delayed viewports
        var chunksToUpdate = new Dictionary<ChunkResolution, HashSet<VecI>>()
        {
            [ChunkResolution.Full] = new(),
            [ChunkResolution.Half] = new(),
            [ChunkResolution.Quarter] = new(),
            [ChunkResolution.Eighth] = new()
        };
        
        var chunksOnDelayedViewports = new Dictionary<ChunkResolution, HashSet<VecI>>()
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
            if (viewport.Delayed)
                chunksOnDelayedViewports[viewport.Resolution].UnionWith(viewportChunks);
            else
                chunksToUpdate[viewport.Resolution].UnionWith(viewportChunks);
        }

        // exclude the chunks that don't need to be updated, remove chunks that will be updated from postponed
        foreach (var (res, postponed) in globalPostponedChunks)
        {
            chunksToUpdate[res].IntersectWith(postponed);
            chunksOnDelayedViewports[res].IntersectWith(postponed);
            postponed.ExceptWith(chunksToUpdate[res]);
        }
        
        // decide what to do about the delayed chunks
        if (renderDelayed)
        {
            foreach (var (res, postponed) in globalPostponedChunks)
            {
                chunksToUpdate[res].UnionWith(chunksOnDelayedViewports[res]);
                postponed.ExceptWith(chunksOnDelayedViewports[res]);
                globalPostponedForDelayed[res] = new HashSet<VecI>(postponed);
            }
        }
        else
        {
            foreach (var (res, postponed) in globalPostponedChunks)
            {
                chunksOnDelayedViewports[res].IntersectWith(globalPostponedForDelayed[res]);
                globalPostponedForDelayed[res].ExceptWith(chunksOnDelayedViewports[res]);
                
                chunksToUpdate[res].UnionWith(chunksOnDelayedViewports[res]);
                postponed.ExceptWith(chunksOnDelayedViewports[res]);
            }
        }

        return chunksToUpdate;
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
        AddChunks(chunkGatherer.imagePreviewChunks, previewDelayedChunks);
        AddChunks(chunkGatherer.maskPreviewChunks, maskPreviewDelayedChunks);
        if (postpone)
            return (new(), new());
        var result = (previewPostponedChunks: previewDelayedChunks, maskPostponedChunks: maskPreviewDelayedChunks);
        previewDelayedChunks = new();
        maskPreviewDelayedChunks = new();
        return result;
    }

    private List<IRenderInfo> Render(AffectedChunkGatherer chunkGatherer, bool updateDelayed)
    {
        Dictionary<ChunkResolution, HashSet<VecI>> chunksToRerender = FindGlobalChunksToRerender(chunkGatherer, updateDelayed);

        List<IRenderInfo> infos = new();
        UpdateMainImage(chunksToRerender, infos);

        var (imagePreviewChunksToRerender, maskPreviewChunksToRerender) = FindPreviewChunksToRerender(chunkGatherer, !updateDelayed);
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
                        (chunk, ChunkResolution.Full, memberVM.PreviewSurface, pos, SmoothReplacingPaint))
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
                        memberVM.PreviewSurface.Canvas.DrawSurface(rendered.AsT0.Surface.SkiaSurface, pos, SmoothReplacingPaint);
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
                    (chunk, ChunkResolution.Full, memberVM.MaskPreviewSurface, pos, SmoothReplacingPaint);
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
            (EmptyChunk _) =>
            {
                var pos = chunkPos * resolution.PixelSize();
                screenSurface.Canvas.DrawRect(pos.X, pos.Y, resolution.PixelSize(), resolution.PixelSize(), ClearPaint);
            });
    }
}
