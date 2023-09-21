#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Models.DocumentModels;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.Models.Rendering.RenderInfos;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;

namespace PixiEditor.AvaloniaUI.Models.Rendering;
internal class MemberPreviewUpdater
{
    private const float smoothingThreshold = 1.5f;

    private readonly IDocument doc;
    private readonly DocumentInternalParts internals;

    private Dictionary<Guid, RectI> lastMainPreviewTightBounds = new();
    private Dictionary<Guid, RectI> lastMaskPreviewTightBounds = new();

    private Dictionary<Guid, AffectedArea> mainPreviewAreasAccumulator = new();
    private Dictionary<Guid, AffectedArea> maskPreviewAreasAccumulator = new();

    private static readonly Paint SmoothReplacingPaint = new() { BlendMode = BlendMode.Src, FilterQuality = FilterQuality.Medium, IsAntiAliased = true };
    private static readonly Paint ReplacingPaint = new() { BlendMode = BlendMode.Src };
    private static readonly Paint ClearPaint = new() { BlendMode = BlendMode.Src, Color = DrawingApi.Core.ColorsImpl.Colors.Transparent };

    public MemberPreviewUpdater(IDocument doc, DocumentInternalParts internals)
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
        AddAreasToAccumulator(chunkGatherer);
        if (!rerenderPreviews)
            return new List<IRenderInfo>();

        Dictionary<Guid, (VecI previewSize, RectI tightBounds)?>? changedMainPreviewBounds = null;
        Dictionary<Guid, (VecI previewSize, RectI tightBounds)?>? changedMaskPreviewBounds = null;
        await Task.Run(() =>
        {
            changedMainPreviewBounds = FindChangedTightBounds(false);
            changedMaskPreviewBounds = FindChangedTightBounds(true);
        }).ConfigureAwait(true);

        RecreatePreviewBitmaps(changedMainPreviewBounds!, changedMaskPreviewBounds!);
        var renderInfos = await Task.Run(() => Render(changedMainPreviewBounds!, changedMaskPreviewBounds)).ConfigureAwait(true);

        CleanupUnusedTightBounds();

        foreach (var a in changedMainPreviewBounds)
        {
            if (a.Value is not null)
                lastMainPreviewTightBounds[a.Key] = a.Value.Value.tightBounds;
            else
                lastMainPreviewTightBounds.Remove(a.Key);
        }

        foreach (var a in changedMaskPreviewBounds)
        {
            if (a.Value is not null)
                lastMaskPreviewTightBounds[a.Key] = a.Value.Value.tightBounds;
            else
                lastMaskPreviewTightBounds.Remove(a.Key);
        }

        return renderInfos;
    }

    /// <summary>
    /// Don't call this outside ActionAccumulator
    /// </summary>
    public List<IRenderInfo> UpdateGatheredChunksSync
        (AffectedAreasGatherer chunkGatherer, bool rerenderPreviews)
    {
        AddAreasToAccumulator(chunkGatherer);
        if (!rerenderPreviews)
            return new List<IRenderInfo>();

        var changedMainPreviewBounds = FindChangedTightBounds(false);
        var changedMaskPreviewBounds = FindChangedTightBounds(true);

        RecreatePreviewBitmaps(changedMainPreviewBounds, changedMaskPreviewBounds);
        var renderInfos = Render(changedMainPreviewBounds, changedMaskPreviewBounds);

        CleanupUnusedTightBounds();

        foreach (var a in changedMainPreviewBounds)
        {
            if (a.Value is not null)
                lastMainPreviewTightBounds[a.Key] = a.Value.Value.tightBounds;
        }

        foreach (var a in changedMaskPreviewBounds)
        {
            if (a.Value is not null)
                lastMaskPreviewTightBounds[a.Key] = a.Value.Value.tightBounds;
        }

        return renderInfos;
    }

    /// <summary>
    /// Cleans up <see cref="lastMainPreviewTightBounds"/> and <see cref="lastMaskPreviewTightBounds"/> to get rid of tight bounds that belonged to now deleted layers
    /// </summary>
    private void CleanupUnusedTightBounds()
    {
        Dictionary<Guid, RectI> clearedLastMainPreviewTightBounds = new Dictionary<Guid, RectI>();
        Dictionary<Guid, RectI> clearedLastMaskPreviewTightBounds = new Dictionary<Guid, RectI>();

        internals.Tracker.Document.ForEveryReadonlyMember(member =>
        {
            if (lastMainPreviewTightBounds.ContainsKey(member.GuidValue))
                clearedLastMainPreviewTightBounds.Add(member.GuidValue, lastMainPreviewTightBounds[member.GuidValue]);
            if (lastMaskPreviewTightBounds.ContainsKey(member.GuidValue))
                clearedLastMaskPreviewTightBounds.Add(member.GuidValue, lastMaskPreviewTightBounds[member.GuidValue]);
        });

        lastMainPreviewTightBounds = clearedLastMainPreviewTightBounds;
        lastMaskPreviewTightBounds = clearedLastMaskPreviewTightBounds;
    }

    /// <summary>
    /// Unions the areas inside <see cref="mainPreviewAreasAccumulator"/> and <see cref="maskPreviewAreasAccumulator"/> with the newly updated areas
    /// </summary>
    private void AddAreasToAccumulator(AffectedAreasGatherer areasGatherer)
    {
        AddAreas(areasGatherer.ImagePreviewAreas, mainPreviewAreasAccumulator);
        AddAreas(areasGatherer.MaskPreviewAreas, maskPreviewAreasAccumulator);
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

    /// <summary>
    /// Looks at the accumulated areas and determines which members need to have their preview bitmaps resized or deleted
    /// </summary>
    private Dictionary<Guid, (VecI previewSize, RectI tightBounds)?> FindChangedTightBounds(bool forMasks)
    {
        // VecI? == null stands for "layer is empty, the preview needs to be deleted"
        Dictionary<Guid, (VecI previewSize, RectI tightBounds)?> newPreviewBitmapSizes = new();

        var targetAreas = forMasks ? maskPreviewAreasAccumulator : mainPreviewAreasAccumulator;
        var targetLastBounds = forMasks ? lastMaskPreviewTightBounds : lastMainPreviewTightBounds;
        foreach (var (guid, area) in targetAreas)
        {
            var member = internals.Tracker.Document.FindMember(guid);
            if (member is null)
                continue;

            if (forMasks && member.Mask is null)
            {
                newPreviewBitmapSizes.Add(guid, null);
                continue;
            }

            RectI? tightBounds = GetOrFindMemberTightBounds(member, area, forMasks);
            RectI? maybeLastBounds = targetLastBounds.TryGetValue(guid, out RectI lastBounds) ? lastBounds : null;
            if (tightBounds == maybeLastBounds)
                continue;

            if (tightBounds is null)
            {
                newPreviewBitmapSizes.Add(guid, null);
                continue;
            }

            VecI previewSize = StructureHelpers.CalculatePreviewSize(tightBounds.Value.Size);
            newPreviewBitmapSizes.Add(guid, (previewSize, tightBounds.Value));
        }
        return newPreviewBitmapSizes;
    }

    /// <summary>
    /// Recreates the preview bitmaps using the passed sizes (or deletes them when new size is null)
    /// </summary>
    private void RecreatePreviewBitmaps(
        Dictionary<Guid, (VecI previewSize, RectI tightBounds)?> newPreviewSizes, 
        Dictionary<Guid, (VecI previewSize, RectI tightBounds)?> newMaskSizes)
    {
        // update previews
        foreach (var (guid, newSize) in newPreviewSizes)
        {
            IStructureMemberHandler member = doc.StructureHelper.FindOrThrow(guid);

            if (newSize is null)
            {
                member.PreviewSurface?.Dispose();
                member.PreviewSurface = null;
                member.PreviewBitmap = null;
            }
            else
            {
                if (member.PreviewBitmap is not null && member.PreviewBitmap.PixelSize.Width == newSize.Value.previewSize.X && member.PreviewBitmap.PixelSize.Height == newSize.Value.previewSize.Y)
                {
                    member.PreviewSurface!.Canvas.Clear();
                }
                else
                {
                    member.PreviewSurface?.Dispose();
                    member.PreviewBitmap = WriteableBitmapUtility.CreateBitmap(newSize.Value.previewSize);
                    member.PreviewSurface = WriteableBitmapUtility.CreateDrawingSurface(member.PreviewBitmap);
                }
            }

            //TODO: Make sure PreviewBitmap implementation raises PropertyChanged
            //member.OnPropertyChanged(nameof(member.PreviewBitmap));
        }

        // update masks
        foreach (var (guid, newSize) in newMaskSizes)
        {
            IStructureMemberHandler member = doc.StructureHelper.FindOrThrow(guid);

            member.MaskPreviewSurface?.Dispose();
            if (newSize is null)
            {
                member.MaskPreviewSurface = null;
                member.MaskPreviewBitmap = null;
            }
            else
            {
                member.MaskPreviewBitmap = WriteableBitmapUtility.CreateBitmap(newSize.Value.previewSize);
                member.MaskPreviewSurface = WriteableBitmapUtility.CreateDrawingSurface(member.MaskPreviewBitmap);
            }

            //TODO: Make sure MaskPreviewBitmap implementation raises PropertyChanged
            //member.OnPropertyChanged(nameof(member.MaskPreviewBitmap));
        }
    }

    /// <summary>
    /// Returns the previosly known committed tight bounds if there are no reasons to believe they have changed (based on the passed <paramref name="currentlyAffectedArea"/>).
    /// Otherwise, calculates the new bounds via <see cref="FindLayerTightBounds"/> and returns them.
    /// </summary>
    private RectI? GetOrFindMemberTightBounds(IReadOnlyStructureMember member, AffectedArea currentlyAffectedArea, bool forMask)
    {
        if (forMask && member.Mask is null)
            throw new InvalidOperationException();

        RectI? prevTightBounds = null;

        var targetLastCollection = forMask ? lastMaskPreviewTightBounds : lastMainPreviewTightBounds;

        if (targetLastCollection.TryGetValue(member.GuidValue, out RectI tightBounds))
            prevTightBounds = tightBounds;

        if (prevTightBounds is not null && currentlyAffectedArea.GlobalArea is not null && prevTightBounds.Value.ContainsExclusive(currentlyAffectedArea.GlobalArea.Value))
        {
            // if the affected area is fully inside the previous tight bounds, the tight bounds couldn't possibly have changed
            return prevTightBounds.Value;
        }

        return member switch
        {
            IReadOnlyLayer layer => FindLayerTightBounds(layer, forMask),
            IReadOnlyFolder folder => FindFolderTightBounds(folder, forMask),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <summary>
    /// Finds the current committed tight bounds for a layer.
    /// </summary>
    private RectI? FindLayerTightBounds(IReadOnlyLayer layer, bool forMask)
    {
        if (layer.Mask is null && forMask)
            throw new InvalidOperationException();

        IReadOnlyChunkyImage targetImage = forMask ? layer.Mask! : layer.LayerImage;
        return FindImageTightBounds(targetImage);
    }

    /// <summary>
    /// Finds the current committed tight bounds for a folder recursively.
    /// </summary>
    private RectI? FindFolderTightBounds(IReadOnlyFolder folder, bool forMask)
    {
        if (forMask)
        {
            if (folder.Mask is null)
                throw new InvalidOperationException();
            return FindImageTightBounds(folder.Mask);
        }

        RectI? combinedBounds = null;
        foreach (var child in folder.Children)
        {
            RectI? curBounds = null;
            
            if (child is IReadOnlyLayer childLayer)
                curBounds = FindLayerTightBounds(childLayer, false);
            else if (child is IReadOnlyFolder childFolder)
                curBounds = FindFolderTightBounds(childFolder, false);

            if (combinedBounds is null)
                combinedBounds = curBounds;
            else if (curBounds is not null)
                combinedBounds = combinedBounds.Value.Union(curBounds.Value);
        }

        return combinedBounds;
    }

    /// <summary>
    /// Finds the current committed tight bounds for an image in a reasonably efficient way.
    /// Looks at the low-res chunks for large images, meaning the resulting bounds aren't 100% precise.
    /// </summary>
    private RectI? FindImageTightBounds(IReadOnlyChunkyImage targetImage)
    {
        RectI? bounds = targetImage.FindChunkAlignedCommittedBounds();
        if (bounds is null)
            return null;

        int biggest = bounds.Value.Size.LongestAxis;
        ChunkResolution resolution = biggest switch
        {
            > ChunkyImage.FullChunkSize * 9 => ChunkResolution.Eighth,
            > ChunkyImage.FullChunkSize * 5 => ChunkResolution.Quarter,
            > ChunkyImage.FullChunkSize * 3 => ChunkResolution.Half,
            _ => ChunkResolution.Full,
        };
        return targetImage.FindTightCommittedBounds(resolution);
    }

    /// <summary>
    /// Re-renders changed chunks using <see cref="mainPreviewAreasAccumulator"/> and <see cref="maskPreviewAreasAccumulator"/> along with the passed lists of bitmaps that need full re-render.
    /// </summary>
    private List<IRenderInfo> Render(
        Dictionary<Guid, (VecI previewSize, RectI tightBounds)?> recreatedMainPreviewSizes,
        Dictionary<Guid, (VecI previewSize, RectI tightBounds)?> recreatedMaskPreviewSizes)
    {
        List<IRenderInfo> infos = new();

        var (mainPreviewChunksToRerender, maskPreviewChunksToRerender) = GetChunksToRerenderAndResetAccumulator();

        RenderWholeCanvasPreview(mainPreviewChunksToRerender, maskPreviewChunksToRerender, infos);
        RenderMainPreviews(mainPreviewChunksToRerender, recreatedMainPreviewSizes, infos);
        RenderMaskPreviews(maskPreviewChunksToRerender, recreatedMaskPreviewSizes, infos);

        return infos;

        // asynchronously re-render changed chunks (where tight bounds didn't change) or the whole preview image (where the tight bounds did change)

        // don't forget to get rid of the bitmap recreation code in DocumentUpdater
    }

    private (Dictionary<Guid, AffectedArea> main, Dictionary<Guid, AffectedArea> mask) GetChunksToRerenderAndResetAccumulator()
    {
        var result = (mainPreviewPostponedChunks: mainPreviewAreasAccumulator, maskPreviewPostponedChunks: maskPreviewAreasAccumulator);
        mainPreviewAreasAccumulator = new();
        maskPreviewAreasAccumulator = new();
        return result;
    }

    /// <summary>
    /// Re-renders the preview of the whole canvas which is shown as the tab icon
    /// </summary>
    private void RenderWholeCanvasPreview(Dictionary<Guid, AffectedArea> mainPreviewChunks, Dictionary<Guid, AffectedArea> maskPreviewChunks, List<IRenderInfo> infos)
    {
        var cumulative = mainPreviewChunks
            .Concat(maskPreviewChunks)
            .Aggregate(new AffectedArea(), (set, pair) =>
        {
            set.UnionWith(pair.Value);
            return set;
        });
        if (cumulative.GlobalArea is null)
            return;

        var previewSize = StructureHelpers.CalculatePreviewSize(internals.Tracker.Document.Size);
        float scaling = (float)previewSize.X / doc.SizeBindable.X;

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

    private void RenderMainPreviews(
        Dictionary<Guid, AffectedArea> mainPreviewChunks, 
        Dictionary<Guid, (VecI previewSize, RectI tightBounds)?> recreatedPreviewSizes, 
        List<IRenderInfo> infos)
    {
        foreach (var guid in mainPreviewChunks.Select(a => a.Key).Concat(recreatedPreviewSizes.Select(a => a.Key)))
        {
            // find the true affected area
            AffectedArea? affArea = null;
            RectI? tightBounds = null;
            
            if (mainPreviewChunks.TryGetValue(guid, out AffectedArea areaFromChunks))
                affArea = areaFromChunks;

            if (recreatedPreviewSizes.TryGetValue(guid, out (VecI _, RectI tightBounds)? value))
            {
                if (value is null)
                    continue;
                tightBounds = value.Value.tightBounds;
                affArea = new AffectedArea(OperationHelper.FindChunksTouchingRectangle(value.Value.tightBounds, ChunkyImage.FullChunkSize), value.Value.tightBounds);
            }

            if (affArea is null || affArea.Value.GlobalArea is null || affArea.Value.GlobalArea.Value.IsZeroOrNegativeArea)
                continue;

            // re-render the area
            var memberVM = doc.StructureHelper.Find(guid);
            if (memberVM is null || memberVM.PreviewSurface is null)
                continue;

            if (tightBounds is null)
                tightBounds = lastMainPreviewTightBounds[guid];

            var member = internals.Tracker.Document.FindMemberOrThrow(guid);

            var previewSize = StructureHelpers.CalculatePreviewSize(tightBounds.Value.Size);
            float scaling = (float)previewSize.X / tightBounds.Value.Width;
            VecI position = tightBounds.Value.Pos;

            if (memberVM is ILayerHandler)
            {
                RenderLayerMainPreview((IReadOnlyLayer)member, memberVM, affArea.Value, position, scaling);
                infos.Add(new PreviewDirty_RenderInfo(guid));
            }
            else if (memberVM is IFolderHandler)
            {
                RenderFolderMainPreview((IReadOnlyFolder)member, memberVM, affArea.Value, position, scaling);
                infos.Add(new PreviewDirty_RenderInfo(guid));
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }

    /// <summary>
    /// Re-render the <paramref name="area"/> of the main preview of the <paramref name="memberVM"/> folder
    /// </summary>
    private void RenderFolderMainPreview(IReadOnlyFolder folder, IStructureMemberHandler memberVM, AffectedArea area, VecI position, float scaling)
    {
        memberVM.PreviewSurface.Canvas.Save();
        memberVM.PreviewSurface.Canvas.Scale(scaling);
        memberVM.PreviewSurface.Canvas.Translate(-position);
        memberVM.PreviewSurface.Canvas.ClipRect((RectD)area.GlobalArea);
        foreach (var chunk in area.Chunks)
        {
            var pos = chunk * ChunkResolution.Full.PixelSize();
            // drawing in full res here is kinda slow
            // we could switch to a lower resolution based on (canvas size / preview size) to make it run faster
            OneOf<Chunk, EmptyChunk> rendered = ChunkRenderer.MergeWholeStructure(chunk, ChunkResolution.Full, folder);
            if (rendered.IsT0)
            {
                memberVM.PreviewSurface.Canvas.DrawSurface(rendered.AsT0.Surface.DrawingSurface, pos, scaling < smoothingThreshold ? SmoothReplacingPaint : ReplacingPaint);
                rendered.AsT0.Dispose();
            }
            else
            {
                memberVM.PreviewSurface.Canvas.DrawRect(pos.X, pos.Y, ChunkResolution.Full.PixelSize(), ChunkResolution.Full.PixelSize(), ClearPaint);
            }
        }
        memberVM.PreviewSurface.Canvas.Restore();
    }

    /// <summary>
    /// Re-render the <paramref name="area"/> of the main preview of the <paramref name="memberVM"/> layer
    /// </summary>
    private void RenderLayerMainPreview(IReadOnlyLayer layer, IStructureMemberHandler memberVM, AffectedArea area, VecI position, float scaling)
    {
        memberVM.PreviewSurface.Canvas.Save();
        memberVM.PreviewSurface.Canvas.Scale(scaling);
        memberVM.PreviewSurface.Canvas.Translate(-position);
        memberVM.PreviewSurface.Canvas.ClipRect((RectD)area.GlobalArea);

        foreach (var chunk in area.Chunks)
        {
            var pos = chunk * ChunkResolution.Full.PixelSize();
            if (!layer.LayerImage.DrawCommittedChunkOn(chunk, ChunkResolution.Full, memberVM.PreviewSurface, pos, scaling < smoothingThreshold ? SmoothReplacingPaint : ReplacingPaint))
                memberVM.PreviewSurface.Canvas.DrawRect(pos.X, pos.Y, ChunkyImage.FullChunkSize, ChunkyImage.FullChunkSize, ClearPaint);
        }

        memberVM.PreviewSurface.Canvas.Restore();
    }

    private void RenderMaskPreviews(
        Dictionary<Guid, AffectedArea> maskPreviewChunks,
        Dictionary<Guid, (VecI previewSize, RectI tightBounds)?> recreatedMaskSizes, 
        List<IRenderInfo> infos)
    {
        foreach (Guid guid in maskPreviewChunks.Select(a => a.Key).Concat(recreatedMaskSizes.Select(a => a.Key)))
        {
            // find the true affected area
            AffectedArea? affArea = null;
            RectI? tightBounds = null;

            if (maskPreviewChunks.TryGetValue(guid, out AffectedArea areaFromChunks))
                affArea = areaFromChunks;

            if (recreatedMaskSizes.TryGetValue(guid, out (VecI _, RectI tightBounds)? value))
            {
                if (value is null)
                    continue;
                tightBounds = value.Value.tightBounds;
                affArea = new AffectedArea(OperationHelper.FindChunksTouchingRectangle(value.Value.tightBounds, ChunkyImage.FullChunkSize), value.Value.tightBounds);
            }

            if (affArea is null || affArea.Value.GlobalArea is null || affArea.Value.GlobalArea.Value.IsZeroOrNegativeArea)
                continue;

            // re-render the area

            var memberVM = doc.StructureHelper.Find(guid);
            if (memberVM is null || !memberVM.HasMaskBindable || memberVM.MaskPreviewSurface is null)
                continue;

            if (tightBounds is null)
                tightBounds = lastMainPreviewTightBounds[guid];

            var previewSize = StructureHelpers.CalculatePreviewSize(tightBounds.Value.Size);
            float scaling = (float)previewSize.X / tightBounds.Value.Width;
            VecI position = tightBounds.Value.Pos;

            var member = internals.Tracker.Document.FindMemberOrThrow(guid);

            memberVM.MaskPreviewSurface!.Canvas.Save();
            memberVM.MaskPreviewSurface.Canvas.Scale(scaling);
            memberVM.MaskPreviewSurface.Canvas.Translate(-position);
            memberVM.MaskPreviewSurface.Canvas.ClipRect((RectD)affArea.Value.GlobalArea);
            foreach (var chunk in affArea.Value.Chunks)
            {
                var pos = chunk * ChunkResolution.Full.PixelSize();
                member.Mask!.DrawMostUpToDateChunkOn
                    (chunk, ChunkResolution.Full, memberVM.MaskPreviewSurface, pos, scaling < smoothingThreshold ? SmoothReplacingPaint : ReplacingPaint);
            }

            memberVM.MaskPreviewSurface.Canvas.Restore();
            infos.Add(new MaskPreviewDirty_RenderInfo(guid));
        }
    }
}
