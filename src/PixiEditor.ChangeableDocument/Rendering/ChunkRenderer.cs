using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using OneOf;
using OneOf.Types;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Rendering;

public static class ChunkRenderer
{
    private static SKPaint ReplacingPaint = new SKPaint() { BlendMode = SKBlendMode.Src };
    private static SKPaint ClippingPaint = new SKPaint() { BlendMode = SKBlendMode.DstIn };

    public static OneOf<Chunk, EmptyChunk> MergeWholeStructure(VecI pos, ChunkResolution resolution, IReadOnlyFolder root)
    {
        using (RenderingContext context = new())
            return MergeFolderContents(context, pos, resolution, root, new All());
    }

    public static OneOf<Chunk, EmptyChunk> MergeChosenMembers(VecI pos, ChunkResolution resolution, IReadOnlyFolder root, HashSet<Guid> members)
    {
        using (RenderingContext context = new())
            return MergeFolderContents(context, pos, resolution, root, members);
    }

    private static OneOf<EmptyChunk, Chunk> RenderLayerWithMask
        (RenderingContext context, Chunk targetChunk, VecI chunkPos, ChunkResolution resolution, IReadOnlyLayer layer, OneOf<FilledChunk, EmptyChunk, Chunk> clippingChunk)
    {
        if (
            clippingChunk.IsT1 ||
            !layer.IsVisible ||
            layer.Opacity == 0 ||
            (layer.ReadOnlyMask is not null && !layer.ReadOnlyMask.LatestOrCommittedChunkExists(chunkPos))
            )
            return new EmptyChunk();

        context.UpdateFromMember(layer);

        Chunk renderingResult = Chunk.Create(resolution);
        if (!layer.ReadOnlyLayerImage.DrawMostUpToDateChunkOn(chunkPos, resolution, renderingResult.Surface.SkiaSurface, new(0, 0), context.ReplacingPaintWithOpacity))
        {
            renderingResult.Dispose();
            return new EmptyChunk();
        }

        if (!layer.ReadOnlyMask!.DrawMostUpToDateChunkOn(chunkPos, resolution, renderingResult.Surface.SkiaSurface, new(0, 0), ClippingPaint))
        {
            // should pretty much never happen due to the check above, but you can never be sure with many threads
            renderingResult.Dispose();
            return new EmptyChunk();
        }

        if (clippingChunk.IsT2)
            OperationHelper.ClampAlpha(renderingResult.Surface.SkiaSurface, clippingChunk.AsT2.Surface.SkiaSurface);

        targetChunk.Surface.SkiaSurface.Canvas.DrawSurface(renderingResult.Surface.SkiaSurface, 0, 0, context.BlendModePaint);
        return renderingResult;
    }

    private static OneOf<EmptyChunk, Chunk> RenderLayerSaveResult
        (RenderingContext context, Chunk targetChunk, VecI chunkPos, ChunkResolution resolution, IReadOnlyLayer layer, OneOf<FilledChunk, EmptyChunk, Chunk> clippingChunk)
    {
        if (clippingChunk.IsT1 || !layer.IsVisible || layer.Opacity == 0)
            return new EmptyChunk();

        if (layer.ReadOnlyMask is not null)
            return RenderLayerWithMask(context, targetChunk, chunkPos, resolution, layer, clippingChunk);

        context.UpdateFromMember(layer);
        Chunk renderingResult = Chunk.Create(resolution);
        if (!layer.ReadOnlyLayerImage.DrawMostUpToDateChunkOn(chunkPos, resolution, renderingResult.Surface.SkiaSurface, new(0, 0), context.ReplacingPaintWithOpacity))
        {
            renderingResult.Dispose();
            return new EmptyChunk();
        }

        if (clippingChunk.IsT2)
            OperationHelper.ClampAlpha(renderingResult.Surface.SkiaSurface, clippingChunk.AsT2.Surface.SkiaSurface);
        targetChunk.Surface.SkiaSurface.Canvas.DrawSurface(renderingResult.Surface.SkiaSurface, 0, 0, context.BlendModePaint);
        return renderingResult;
    }

    private static void RenderLayer
        (RenderingContext context, Chunk targetChunk, VecI chunkPos, ChunkResolution resolution, IReadOnlyLayer layer, OneOf<FilledChunk, EmptyChunk, Chunk> clippingChunk)
    {
        if (clippingChunk.IsT1 || !layer.IsVisible || layer.Opacity == 0)
            return;
        if (layer.ReadOnlyMask is not null)
        {
            var result = RenderLayerWithMask(context, targetChunk, chunkPos, resolution, layer, clippingChunk);
            if (result.IsT1)
                result.AsT1.Dispose();
            return;
        }
        // clipping chunk requires a temp chunk anyway so we could as well reuse the code from RenderLayerSaveResult
        if (clippingChunk.IsT2)
        {
            var result = RenderLayerSaveResult(context, targetChunk, chunkPos, resolution, layer, clippingChunk);
            if (result.IsT1)
                result.AsT1.Dispose();
            return;
        }
        context.UpdateFromMember(layer);
        layer.ReadOnlyLayerImage.DrawMostUpToDateChunkOn(chunkPos, resolution, targetChunk.Surface.SkiaSurface, new(0, 0), context.BlendModeOpacityPaint);
    }

    private static OneOf<EmptyChunk, Chunk> RenderFolder(
        RenderingContext context,
        Chunk targetChunk,
        VecI chunkPos,
        ChunkResolution resolution,
        IReadOnlyFolder folder,
        OneOf<FilledChunk, EmptyChunk, Chunk> clippingChunk,
        OneOf<All, HashSet<Guid>> membersToMerge)
    {
        if (
            clippingChunk.IsT1 ||
            !folder.IsVisible ||
            folder.Opacity == 0 ||
            folder.ReadOnlyChildren.Count == 0 ||
            (folder.ReadOnlyMask is not null && !folder.ReadOnlyMask.LatestOrCommittedChunkExists(chunkPos))
            )
            return new EmptyChunk();

        OneOf<Chunk, EmptyChunk> maybeContents = MergeFolderContents(context, chunkPos, resolution, folder, membersToMerge);
        if (maybeContents.IsT1)
            return new EmptyChunk();
        Chunk contents = maybeContents.AsT0;

        if (folder.ReadOnlyMask is not null)
        {
            if (!folder.ReadOnlyMask.DrawMostUpToDateChunkOn(chunkPos, resolution, contents.Surface.SkiaSurface, new(0, 0), ClippingPaint))
            {
                // this shouldn't really happen due to the check above, but another thread could edit the mask in the meantime
                contents.Dispose();
                return new EmptyChunk();
            }
        }

        if (clippingChunk.IsT2)
            OperationHelper.ClampAlpha(contents.Surface.SkiaSurface, clippingChunk.AsT2.Surface.SkiaSurface);
        context.UpdateFromMember(folder);
        contents.Surface.SkiaSurface.Canvas.DrawSurface(contents.Surface.SkiaSurface, 0, 0, context.ReplacingPaintWithOpacity);
        targetChunk.Surface.SkiaSurface.Canvas.DrawSurface(contents.Surface.SkiaSurface, 0, 0, context.BlendModePaint);

        return contents;
    }

    private static OneOf<Chunk, EmptyChunk> MergeFolderContents(
        RenderingContext context,
        VecI chunkPos,
        ChunkResolution resolution,
        IReadOnlyFolder folder,
        OneOf<All, HashSet<Guid>> membersToMerge)
    {
        if (folder.ReadOnlyChildren.Count == 0)
            return new EmptyChunk();

        // clipping to member below doesn't make sense if we are skipping some of them
        bool ignoreClipToBelow = membersToMerge.IsT1;

        Chunk targetChunk = Chunk.Create(resolution);
        targetChunk.Surface.SkiaSurface.Canvas.Clear();

        OneOf<FilledChunk, EmptyChunk, Chunk> clippingChunk = new FilledChunk();
        for (int i = 0; i < folder.ReadOnlyChildren.Count; i++)
        {
            var child = folder.ReadOnlyChildren[i];

            // next child might use clip to member below in which case we need to save the clip image
            bool needToSaveClippingChunk =
                !ignoreClipToBelow &&
                i < folder.ReadOnlyChildren.Count - 1 &&
                !child.ClipToMemberBelow &&
                folder.ReadOnlyChildren[i + 1].ClipToMemberBelow;

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
                    OneOf<EmptyChunk, Chunk> result = RenderLayerSaveResult(context, targetChunk, chunkPos, resolution, layer, clippingChunk);
                    clippingChunk = result.IsT0 ? result.AsT0 : result.AsT1;
                }
                else
                {
                    RenderLayer(context, targetChunk, chunkPos, resolution, layer, clippingChunk);
                }
                continue;
            }

            // folder
            if (child is IReadOnlyFolder innerFolder)
            {
                bool shouldRenderAllChildren = membersToMerge.IsT0 || membersToMerge.AsT1.Contains(innerFolder.GuidValue);
                OneOf<All, HashSet<Guid>> innerMembersToMerge = shouldRenderAllChildren ? new All() : membersToMerge;
                if (needToSaveClippingChunk)
                {
                    OneOf<EmptyChunk, Chunk> result = RenderFolder(context, targetChunk, chunkPos, resolution, innerFolder, clippingChunk, innerMembersToMerge);
                    clippingChunk = result.IsT0 ? result.AsT0 : result.AsT1;
                }
                else
                {
                    RenderFolder(context, targetChunk, chunkPos, resolution, innerFolder, clippingChunk, innerMembersToMerge);
                }
                continue;
            }
        }
        if (clippingChunk.IsT2)
            clippingChunk.AsT2.Dispose();
        return targetChunk;
    }
}
