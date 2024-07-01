using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Rendering;

public static class ChunkRenderer
{
    private static readonly Paint ClippingPaint = new Paint() { BlendMode = BlendMode.DstIn };

    private static RectI? TransformClipRect(RectI? globalClippingRect, ChunkResolution resolution, VecI chunkPos)
    {
        if (globalClippingRect is not RectI rect)
            return null;

        double multiplier = resolution.Multiplier();
        VecI pixelChunkPos = chunkPos * (int)(ChunkyImage.FullChunkSize * multiplier);
        return (RectI?)rect.Scale(multiplier).Translate(-pixelChunkPos).RoundOutwards();
    }

    public static OneOf<Chunk, EmptyChunk> MergeWholeStructure(VecI chunkPos, ChunkResolution resolution, IReadOnlyFolder root, int frame, RectI? globalClippingRect = null)
    {
        using RenderingContext context = new();
        try
        {
            RectI? transformedClippingRect = TransformClipRect(globalClippingRect, resolution, chunkPos);
            return MergeFolderContents(context, chunkPos, resolution, root, frame, new All(), transformedClippingRect);
        }
        catch (ObjectDisposedException)
        {
            return new EmptyChunk();
        }
    }

    public static OneOf<Chunk, EmptyChunk> MergeChosenMembers(VecI chunkPos, ChunkResolution resolution, IReadOnlyFolder root, int frame, HashSet<Guid> members, RectI? globalClippingRect = null)
    {
        using RenderingContext context = new();
        try
        {
            RectI? transformedClippingRect = TransformClipRect(globalClippingRect, resolution, chunkPos);
            return MergeFolderContents(context, chunkPos, resolution, root, frame, members, transformedClippingRect);
        }
        catch (ObjectDisposedException)
        {
            return new EmptyChunk();
        }
    }

    private static OneOf<EmptyChunk, Chunk> RenderLayerWithMask(
        RenderingContext context,
        Chunk targetChunk,
        VecI chunkPos,
        int frame,
        ChunkResolution resolution,
        IReadOnlyLayer layer,
        OneOf<FilledChunk, EmptyChunk, Chunk> clippingChunk,
        RectI? transformedClippingRect)
    {
        if (
            clippingChunk.IsT1 ||
            !layer.IsVisible ||
            layer.Opacity == 0 ||
            (layer.Mask is not null && !layer.Mask.LatestOrCommittedChunkExists(chunkPos))
        )
            return new EmptyChunk();

        context.UpdateFromMember(layer);

        Chunk renderingResult = Chunk.Create(resolution);
        if (transformedClippingRect is not null)
        {
            renderingResult.Surface.DrawingSurface.Canvas.Save();
            renderingResult.Surface.DrawingSurface.Canvas.ClipRect((RectD)transformedClippingRect);
            targetChunk.Surface.DrawingSurface.Canvas.Save();
            targetChunk.Surface.DrawingSurface.Canvas.ClipRect((RectD)transformedClippingRect);
        }

        if (!layer.Rasterize(frame).DrawMostUpToDateChunkOn(chunkPos, resolution, renderingResult.Surface.DrawingSurface, VecI.Zero, context.ReplacingPaintWithOpacity))
        {
            renderingResult.Dispose();
            return new EmptyChunk();
        }

        if (!layer.Mask!.DrawMostUpToDateChunkOn(chunkPos, resolution, renderingResult.Surface.DrawingSurface, VecI.Zero, ClippingPaint))
        {
            // should pretty much never happen due to the check above, but you can never be sure with many threads
            renderingResult.Dispose();
            return new EmptyChunk();
        }

        if (clippingChunk.IsT2)
            OperationHelper.ClampAlpha(renderingResult.Surface.DrawingSurface, clippingChunk.AsT2.Surface.DrawingSurface, transformedClippingRect);

        targetChunk.Surface.DrawingSurface.Canvas.DrawSurface(renderingResult.Surface.DrawingSurface, 0, 0, context.BlendModePaint);
        if (transformedClippingRect is not null)
        {
            renderingResult.Surface.DrawingSurface.Canvas.Restore();
            targetChunk.Surface.DrawingSurface.Canvas.Restore();
        }

        return renderingResult;
    }

    private static OneOf<EmptyChunk, Chunk> RenderLayerSaveResult(
        RenderingContext context,
        Chunk targetChunk,
        VecI chunkPos,
        int frame,
        ChunkResolution resolution,
        IReadOnlyLayer layer,
        OneOf<FilledChunk, EmptyChunk, Chunk> clippingChunk,
        RectI? transformedClippingRect)
    {
        if (clippingChunk.IsT1 || !layer.IsVisible || layer.Opacity == 0)
            return new EmptyChunk();

        if (layer.Mask is not null && layer.MaskIsVisible)
            return RenderLayerWithMask(context, targetChunk, chunkPos, frame, resolution, layer, clippingChunk, transformedClippingRect);

        context.UpdateFromMember(layer);
        Chunk renderingResult = Chunk.Create(resolution);
        if (transformedClippingRect is not null)
        {
            renderingResult.Surface.DrawingSurface.Canvas.Save();
            renderingResult.Surface.DrawingSurface.Canvas.ClipRect((RectD)transformedClippingRect);
            targetChunk.Surface.DrawingSurface.Canvas.Save();
            targetChunk.Surface.DrawingSurface.Canvas.ClipRect((RectD)transformedClippingRect);
        }
        if (!layer.Rasterize(frame).DrawMostUpToDateChunkOn(chunkPos, resolution, renderingResult.Surface.DrawingSurface, VecI.Zero, context.ReplacingPaintWithOpacity))
        {
            renderingResult.Dispose();
            return new EmptyChunk();
        }

        if (clippingChunk.IsT2)
            OperationHelper.ClampAlpha(renderingResult.Surface.DrawingSurface, clippingChunk.AsT2.Surface.DrawingSurface, transformedClippingRect);
        targetChunk.Surface.DrawingSurface.Canvas.DrawSurface(renderingResult.Surface.DrawingSurface, 0, 0, context.BlendModePaint);

        if (transformedClippingRect is not null)
        {
            renderingResult.Surface.DrawingSurface.Canvas.Restore();
            targetChunk.Surface.DrawingSurface.Canvas.Restore();
        }
        return renderingResult;
    }

    private static void RenderLayer(RenderingContext context,
        Chunk targetChunk,
        VecI chunkPos,
        ChunkResolution resolution,
        IReadOnlyLayer layer,
        int frame,
        OneOf<FilledChunk, EmptyChunk, Chunk> clippingChunk,
        RectI? transformedClippingRect)
    {
        if (clippingChunk.IsT1 || !layer.IsVisible || layer.Opacity == 0)
            return;
        if (layer.Mask is not null && layer.MaskIsVisible)
        {
            var result = RenderLayerWithMask(context, targetChunk, chunkPos, frame, resolution, layer, clippingChunk, transformedClippingRect);
            if (result.IsT1)
                result.AsT1.Dispose();
            return;
        }
        // clipping chunk requires a temp chunk anyway so we could as well reuse the code from RenderLayerSaveResult
        if (clippingChunk.IsT2)
        {
            var result = RenderLayerSaveResult(context, targetChunk, chunkPos, frame, resolution, layer, clippingChunk, transformedClippingRect);
            if (result.IsT1)
                result.AsT1.Dispose();
            return;
        }
        context.UpdateFromMember(layer);

        if (transformedClippingRect is not null)
        {
            targetChunk.Surface.DrawingSurface.Canvas.Save();
            targetChunk.Surface.DrawingSurface.Canvas.ClipRect((RectD)transformedClippingRect);
        }
        layer.Rasterize(frame).DrawMostUpToDateChunkOn(chunkPos, resolution, targetChunk.Surface.DrawingSurface, VecI.Zero, context.BlendModeOpacityPaint);
        if (transformedClippingRect is not null)
            targetChunk.Surface.DrawingSurface.Canvas.Restore();
    }

    private static OneOf<EmptyChunk, Chunk> RenderFolder(
        RenderingContext context,
        Chunk targetChunk,
        VecI chunkPos,
        ChunkResolution resolution,
        IReadOnlyFolder folder,
        int frame,
        OneOf<FilledChunk, EmptyChunk, Chunk> clippingChunk,
        OneOf<All, HashSet<Guid>> membersToMerge,
        RectI? transformedClippingRect)
    {
        if (
            clippingChunk.IsT1 ||
            !folder.IsVisible ||
            folder.Opacity == 0 ||
            folder.Children.Count == 0 ||
            (folder.Mask is not null && folder.MaskIsVisible && !folder.Mask.LatestOrCommittedChunkExists(chunkPos))
        )
            return new EmptyChunk();

        OneOf<Chunk, EmptyChunk> maybeContents = MergeFolderContents(context, chunkPos, resolution, folder, frame, membersToMerge, transformedClippingRect);
        if (maybeContents.IsT1)
            return new EmptyChunk();
        Chunk contents = maybeContents.AsT0;

        if (transformedClippingRect is not null)
        {
            contents.Surface.DrawingSurface.Canvas.Save();
            contents.Surface.DrawingSurface.Canvas.ClipRect((RectD)transformedClippingRect);
            targetChunk.Surface.DrawingSurface.Canvas.Save();
            targetChunk.Surface.DrawingSurface.Canvas.ClipRect((RectD)transformedClippingRect);
        }

        if (folder.Mask is not null && folder.MaskIsVisible)
        {
            if (!folder.Mask.DrawMostUpToDateChunkOn(chunkPos, resolution, contents.Surface.DrawingSurface, VecI.Zero, ClippingPaint))
            {
                // this shouldn't really happen due to the check above, but another thread could edit the mask in the meantime
                contents.Dispose();
                return new EmptyChunk();
            }
        }

        if (clippingChunk.IsT2)
            OperationHelper.ClampAlpha(contents.Surface.DrawingSurface, clippingChunk.AsT2.Surface.DrawingSurface, transformedClippingRect);
        context.UpdateFromMember(folder);
        contents.Surface.DrawingSurface.Canvas.DrawSurface(contents.Surface.DrawingSurface, 0, 0, context.ReplacingPaintWithOpacity);
        targetChunk.Surface.DrawingSurface.Canvas.DrawSurface(contents.Surface.DrawingSurface, 0, 0, context.BlendModePaint);

        if (transformedClippingRect is not null)
        {
            contents.Surface.DrawingSurface.Canvas.Restore();
            targetChunk.Surface.DrawingSurface.Canvas.Restore();
        }

        return contents;
    }

    private static OneOf<Chunk, EmptyChunk> MergeFolderContents(
        RenderingContext context,
        VecI chunkPos,
        ChunkResolution resolution,
        IReadOnlyFolder folder,
        int frame,
        OneOf<All, HashSet<Guid>> membersToMerge,
        RectI? transformedClippingRect)
    {
        var folderChildren = folder.Children;
        if (folderChildren.Count == 0)
            return new EmptyChunk();

        Chunk targetChunk = Chunk.Create(resolution);
        targetChunk.Surface.DrawingSurface.Canvas.Clear();

        OneOf<FilledChunk, EmptyChunk, Chunk> clippingChunk = new FilledChunk();
        for (int i = 0; i < folderChildren.Count; i++)
        {
            var child = folderChildren[i];

            // next child might use clip to member below in which case we need to save the clip image
            bool needToSaveClippingChunk =
                i < folderChildren.Count - 1 &&
                !child.ClipToMemberBelow &&
                folderChildren[i + 1].ClipToMemberBelow;

            // if the current member doesn't need a clip, get rid of it
            if (!child.ClipToMemberBelow && !clippingChunk.IsT0)
            {
                if (clippingChunk.IsT2)
                    clippingChunk.AsT2.Dispose();
                clippingChunk = new FilledChunk();
            }

            // layer
            if (child is IReadOnlyLayer layer && (membersToMerge.IsT0 || membersToMerge.AsT1.Contains(layer.GuidValue)))
            {
                if (needToSaveClippingChunk)
                {
                    OneOf<EmptyChunk, Chunk> result = RenderLayerSaveResult(context, targetChunk, chunkPos, frame, resolution, layer, clippingChunk, transformedClippingRect);
                    clippingChunk = result.IsT0 ? result.AsT0 : result.AsT1;
                }
                else
                {
                    RenderLayer(context, targetChunk, chunkPos, resolution, layer, frame, clippingChunk, transformedClippingRect);
                }
                continue;
            }
            else if (child is IReadOnlyLayer && needToSaveClippingChunk)
            {
                clippingChunk = new FilledChunk();
            }

            // folder
            if (child is IReadOnlyFolder innerFolder)
            {
                bool shouldRenderAllChildren = membersToMerge.IsT0 || membersToMerge.AsT1.Contains(innerFolder.GuidValue);
                OneOf<All, HashSet<Guid>> innerMembersToMerge = shouldRenderAllChildren ? new All() : membersToMerge;
                if (needToSaveClippingChunk)
                {
                    OneOf<EmptyChunk, Chunk> result = RenderFolder(context, targetChunk, chunkPos, resolution, innerFolder, frame, clippingChunk, innerMembersToMerge, transformedClippingRect);
                    clippingChunk = result.IsT0 ? result.AsT0 : result.AsT1;
                }
                else
                {
                    RenderFolder(context, targetChunk, chunkPos, resolution, innerFolder, frame, clippingChunk, innerMembersToMerge, transformedClippingRect);
                }
            }
        }
        if (clippingChunk.IsT2)
            clippingChunk.AsT2.Dispose();
        return targetChunk;
    }
}
