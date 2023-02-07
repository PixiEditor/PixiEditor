using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Rendering.RenderInfos;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Models.Rendering;
internal class MemberPreviewUpdater
{
    private readonly DocumentViewModel doc;
    private readonly DocumentInternalParts internals;

    private Dictionary<Guid, AffectedArea> previewDelayedAreas = new();
    private Dictionary<Guid, AffectedArea> maskPreviewDelayedAreas = new();

    private static readonly Paint SmoothReplacingPaint = new() { BlendMode = BlendMode.Src, FilterQuality = FilterQuality.Medium, IsAntiAliased = true };
    private static readonly Paint ClearPaint = new() { BlendMode = BlendMode.Src, Color = PixiEditor.DrawingApi.Core.ColorsImpl.Colors.Transparent };

    public MemberPreviewUpdater(DocumentViewModel doc, DocumentInternalParts internals)
    {
        this.doc = doc;
        this.internals = internals;
    }

    /// <summary>
    /// Don't call this outside ActionAccumulator
    /// </summary>
    public async Task<List<IRenderInfo>> UpdateGatheredChunks
        (AffectedAreasGatherer chunkGatherer, bool rerenderPreviews)
    {
        return await Task.Run(() => Render(chunkGatherer, rerenderPreviews)).ConfigureAwait(true);
    }

    /// <summary>
    /// Don't call this outside ActionAccumulator
    /// </summary>
    public List<IRenderInfo> UpdateGatheredChunksSync
        (AffectedAreasGatherer chunkGatherer, bool rerenderPreviews)
    {
        return Render(chunkGatherer, rerenderPreviews);
    }

    private List<IRenderInfo> Render(AffectedAreasGatherer chunkGatherer, bool rerenderPreviews)
    {
        List<IRenderInfo> infos = new();

        var (imagePreviewChunksToRerender, maskPreviewChunksToRerender) = FindPreviewChunksToRerender(chunkGatherer, !rerenderPreviews);
        var previewSize = StructureMemberViewModel.CalculatePreviewSize(internals.Tracker.Document.Size);
        float scaling = (float)previewSize.X / doc.SizeBindable.X;
        UpdateImagePreviews(imagePreviewChunksToRerender, scaling, infos);
        UpdateMaskPreviews(maskPreviewChunksToRerender, scaling, infos);

        return infos;
    }

    private static void AddAreas(Dictionary<Guid, AffectedArea> from, Dictionary<Guid, AffectedArea> to)
    {
        foreach ((Guid guid, AffectedArea area) in from)
        {
            if (!to.ContainsKey(guid))
                to[guid] = new AffectedArea();
            var toArea = to[guid];
            toArea.UnionWith(area);
            to[guid] = toArea;
        }
    }

    private (Dictionary<Guid, AffectedArea> image, Dictionary<Guid, AffectedArea> mask) FindPreviewChunksToRerender
        (AffectedAreasGatherer areasGatherer, bool delay)
    {
        AddAreas(areasGatherer.ImagePreviewAreas, previewDelayedAreas);
        AddAreas(areasGatherer.MaskPreviewAreas, maskPreviewDelayedAreas);
        if (delay)
            return (new(), new());
        var result = (previewPostponedChunks: previewDelayedAreas, maskPostponedChunks: maskPreviewDelayedAreas);
        previewDelayedAreas = new();
        maskPreviewDelayedAreas = new();
        return result;
    }

    private void UpdateImagePreviews(Dictionary<Guid, AffectedArea> imagePreviewChunks, float scaling, List<IRenderInfo> infos)
    {
        UpdateWholeCanvasPreview(imagePreviewChunks, scaling, infos);
        UpdateMembersImagePreviews(imagePreviewChunks, scaling, infos);
    }

    private void UpdateWholeCanvasPreview(Dictionary<Guid, AffectedArea> imagePreviewChunks, float scaling, List<IRenderInfo> infos)
    {
        // update preview of the whole canvas
        var cumulative = imagePreviewChunks.Aggregate(new AffectedArea(), (set, pair) =>
        {
            set.UnionWith(pair.Value);
            return set;
        });
        if (cumulative.GlobalArea is null)
            return;

        bool somethingChanged = false;
        foreach (var chunkPos in cumulative.Chunks)
        {
            somethingChanged = true;
            ChunkResolution resolution = scaling switch
            {
                > 1 / 2f => ChunkResolution.Full,
                > 1 / 4f => ChunkResolution.Half,
                > 1 / 8f => ChunkResolution.Quarter,
                _ => ChunkResolution.Eighth,
            };
            var pos = chunkPos * resolution.PixelSize();
            var rendered = ChunkRenderer.MergeWholeStructure(chunkPos, resolution, internals.Tracker.Document.StructureRoot);
            doc.PreviewSurface.Canvas.Save();
            doc.PreviewSurface.Canvas.Scale(scaling);
            doc.PreviewSurface.Canvas.ClipRect((RectD)cumulative.GlobalArea);
            doc.PreviewSurface.Canvas.Scale(1 / (float)resolution.Multiplier());
            if (rendered.IsT1)
            {
                doc.PreviewSurface.Canvas.DrawRect(pos.X, pos.Y, resolution.PixelSize(), resolution.PixelSize(), ClearPaint);
            }
            else if (rendered.IsT0)
            {
                using var renderedChunk = rendered.AsT0;
                renderedChunk.DrawOnSurface(doc.PreviewSurface, pos, SmoothReplacingPaint);
            }
            doc.PreviewSurface.Canvas.Restore();
        }
        if (somethingChanged)
            infos.Add(new CanvasPreviewDirty_RenderInfo());
    }

    private void UpdateMembersImagePreviews(Dictionary<Guid, AffectedArea> imagePreviewChunks, float scaling, List<IRenderInfo> infos)
    {
        foreach (var (guid, area) in imagePreviewChunks)
        {
            if (area.GlobalArea is null)
                continue;
            var memberVM = doc.StructureHelper.Find(guid);
            if (memberVM is null)
                continue;
            var member = internals.Tracker.Document.FindMemberOrThrow(guid);

            memberVM.PreviewSurface.Canvas.Save();
            memberVM.PreviewSurface.Canvas.Scale(scaling);
            memberVM.PreviewSurface.Canvas.ClipRect((RectD)area.GlobalArea);
            if (memberVM is LayerViewModel)
            {
                var layer = (IReadOnlyLayer)member;
                foreach (var chunk in area.Chunks)
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
                foreach (var chunk in area.Chunks)
                {
                    var pos = chunk * ChunkResolution.Full.PixelSize();
                    // drawing in full res here is kinda slow
                    // we could switch to a lower resolution based on (canvas size / preview size) to make it run faster
                    OneOf<Chunk, EmptyChunk> rendered = ChunkRenderer.MergeWholeStructure(chunk, ChunkResolution.Full, folder);
                    if (rendered.IsT0)
                    {
                        memberVM.PreviewSurface.Canvas.DrawSurface(rendered.AsT0.Surface.DrawingSurface, pos, SmoothReplacingPaint);
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

    private void UpdateMaskPreviews(Dictionary<Guid, AffectedArea> maskPreviewChunks, float scaling, List<IRenderInfo> infos)
    {
        foreach (var (guid, area) in maskPreviewChunks)
        {
            if (area.GlobalArea is null)
                continue;
            var memberVM = doc.StructureHelper.Find(guid);
            if (memberVM is null || !memberVM.HasMaskBindable)
                continue;

            var member = internals.Tracker.Document.FindMemberOrThrow(guid);
            memberVM.MaskPreviewSurface!.Canvas.Save();
            memberVM.MaskPreviewSurface.Canvas.Scale(scaling);
            memberVM.MaskPreviewSurface.Canvas.ClipRect((RectD)area.GlobalArea);
            foreach (var chunk in area.Chunks)
            {
                var pos = chunk * ChunkResolution.Full.PixelSize();
                member.Mask!.DrawMostUpToDateChunkOn
                    (chunk, ChunkResolution.Full, memberVM.MaskPreviewSurface, pos, SmoothReplacingPaint);
            }

            memberVM.MaskPreviewSurface.Canvas.Restore();
            infos.Add(new MaskPreviewDirty_RenderInfo(guid));
        }
    }
}
