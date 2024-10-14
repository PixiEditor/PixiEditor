#nullable enable

using System.Diagnostics.CodeAnalysis;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Helpers;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Rendering.RenderInfos;
using PixiEditor.Numerics;

namespace PixiEditor.Models.Rendering;

internal class MemberPreviewUpdater
{
    private const float smoothingThreshold = 1.5f;

    private readonly IDocument doc;
    private readonly DocumentInternalParts internals;

    private static readonly Paint SmoothReplacingPaint = new()
    {
        BlendMode = BlendMode.Src, FilterQuality = FilterQuality.Medium, IsAntiAliased = true
    };

    private static readonly Paint ReplacingPaint = new() { BlendMode = BlendMode.Src };

    private static readonly Paint ClearPaint =
        new() { BlendMode = BlendMode.Src, Color = DrawingApi.Core.ColorsImpl.Colors.Transparent };

    public MemberPreviewUpdater(IDocument doc, DocumentInternalParts internals)
    {
        this.doc = doc;
        this.internals = internals;
    }

    public List<IRenderInfo> UpdatePreviews(bool rerenderPreviews, IEnumerable<Guid> keys)
    {
        if (!rerenderPreviews)
            return new List<IRenderInfo>();

        var renderInfos = UpdatePreviewPainters(keys);

        return renderInfos;
    }

    /// <summary>
    /// Finds the current committed tight bounds for a layer.
    /// </summary>
    private RectI? FindLayerTightBounds(IReadOnlyLayerNode layer, int frame, bool forMask)
    {
        if (layer.EmbeddedMask is null && forMask)
            throw new InvalidOperationException();

        if (layer.EmbeddedMask is not null && forMask)
            return FindImageTightBoundsFast(layer.EmbeddedMask);

        if (layer is IReadOnlyImageNode raster)
        {
            return FindImageTightBoundsFast(raster.GetLayerImageAtFrame(frame));
        }

        return (RectI?)layer.GetTightBounds(frame);
    }

    /// <summary>
    /// Finds the current committed tight bounds for a folder recursively.
    /// </summary>
    private RectI? FindFolderTightBounds(IReadOnlyFolderNode folder, int frame, bool forMask)
    {
        if (forMask)
        {
            if (folder.EmbeddedMask is null)
                throw new InvalidOperationException();
            return FindImageTightBoundsFast(folder.EmbeddedMask);
        }

        return (RectI?)folder.GetTightBounds(frame);
    }

    /// <summary>
    /// Finds the current committed tight bounds for an image in a reasonably efficient way.
    /// Looks at the low-res chunks for large images, meaning the resulting bounds aren't 100% precise.
    /// </summary>
    private RectI? FindImageTightBoundsFast(IReadOnlyChunkyImage targetImage)
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
    /// <param name="members"></param>
    private List<IRenderInfo> UpdatePreviewPainters(IEnumerable<Guid> members)
    {
        List<IRenderInfo> infos = new();

        RenderWholeCanvasPreview(infos);
        RenderMainPreviews(infos, members);
        RenderMaskPreviews(infos);
        RenderNodePreviews(infos);

        return infos;
    }

    /// <summary>
    /// Re-renders the preview of the whole canvas which is shown as the tab icon
    /// </summary>
    private void RenderWholeCanvasPreview(List<IRenderInfo> infos)
    {
        var previewSize = StructureHelpers.CalculatePreviewSize(internals.Tracker.Document.Size);
        float scaling = (float)previewSize.X / doc.SizeBindable.X;

        //infos.Add(new CanvasPreviewDirty_RenderInfo());
    }

    private void RenderMainPreviews(List<IRenderInfo> infos, IEnumerable<Guid> members)
    {
        Guid[] memberGuids = members.ToArray();
        foreach (var node in doc.NodeGraphHandler.AllNodes)
        {
            if (node is IStructureMemberHandler structureMemberHandler)
            {
                if (!memberGuids.Contains(node.Id))
                    continue;
                
                if (structureMemberHandler.PreviewPainter == null)
                {
                    var member = internals.Tracker.Document.FindMember(node.Id);
                    if (member is not IPreviewRenderable previewRenderable)
                        continue;

                    structureMemberHandler.PreviewPainter = new PreviewPainter(previewRenderable, structureMemberHandler.TightBounds);
                    structureMemberHandler.PreviewPainter.Repaint();
                }
                else
                {
                    structureMemberHandler.PreviewPainter.Bounds = structureMemberHandler.TightBounds;
                    structureMemberHandler.PreviewPainter.Repaint();
                }
            }
        }
    }

    private void RenderGroupPreview(IKeyFrameHandler keyFrame, IStructureMemberHandler memberVM,
        IReadOnlyStructureNode member, [DisallowNull] AffectedArea? affArea, VecI position, float scaling)
    {
        /*bool isEditingRootImage = !member.KeyFrames.Any(x => x.IsInFrame(doc.AnimationHandler.ActiveFrameBindable));
        if (!isEditingRootImage && keyFrame.PreviewSurface is not null)
            return;

        if (keyFrame.PreviewSurface == null ||
            keyFrame.PreviewSurface.Size != memberVM.PreviewPainter.Size)
        {
            keyFrame.PreviewSurface?.Dispose();
            keyFrame.PreviewSurface = new Texture(memberVM.PreviewPainter.Size);
        }

        RenderLayerMainPreview((IReadOnlyLayerNode)member, keyFrame.PreviewSurface, affArea.Value,
            position, scaling, 0);*/
    }

    /// <summary>
    /// Re-render the <paramref name="area"/> of the main preview of the <paramref name="memberVM"/> folder
    /// </summary>
    private void RenderFolderMainPreview(IReadOnlyFolderNode folder, IStructureMemberHandler memberVM,
        AffectedArea area,
        VecI position, float scaling)
    {
        /*QueueRender(() =>
        {
            memberVM.PreviewSurface.DrawingSurface.Canvas.Save();
            memberVM.PreviewSurface.DrawingSurface.Canvas.Scale(scaling);
            memberVM.PreviewSurface.DrawingSurface.Canvas.Translate(-position);
            memberVM.PreviewSurface.DrawingSurface.Canvas.ClipRect((RectD)area.GlobalArea);
            foreach (var chunk in area.Chunks)
            {
                var pos = chunk * ChunkResolution.Full.PixelSize();
                // drawing in full res here is kinda slow
                // we could switch to a lower resolution based on (canvas size / preview size) to make it run faster
                HashSet<Guid> layers = folder.GetLayerNodeGuids();

                OneOf<Chunk, EmptyChunk> rendered;

                if (layers.Count == 0)
                {
                    rendered = new EmptyChunk();
                }
                else
                {
                    rendered = doc.Renderer.RenderLayersChunk(chunk, ChunkResolution.Full,
                        doc.AnimationHandler.ActiveFrameTime, layers,
                        null);
                }

                if (rendered.IsT0)
                {
                    memberVM.PreviewSurface.DrawingSurface.Canvas.DrawSurface(rendered.AsT0.Surface.DrawingSurface, pos,
                        scaling < smoothingThreshold ? SmoothReplacingPaint : ReplacingPaint);
                    rendered.AsT0.Dispose();
                }
                else
                {
                    memberVM.PreviewSurface.DrawingSurface.Canvas.DrawRect(pos.X, pos.Y,
                        ChunkResolution.Full.PixelSize(),
                        ChunkResolution.Full.PixelSize(), ClearPaint);
                }
            }

            memberVM.PreviewSurface.DrawingSurface.Canvas.Restore();
        });*/
    }

    private void RenderAnimationFramePreview(IReadOnlyImageNode node, IKeyFrameHandler keyFrameVM, AffectedArea area)
    {
        if (keyFrameVM.PreviewSurface is null)
        {
            keyFrameVM.PreviewSurface =
                new Texture(StructureHelpers.CalculatePreviewSize(internals.Tracker.Document.Size));
        }

        /*QueueRender(() =>
        {
            keyFrameVM.PreviewSurface!.DrawingSurface.Canvas.Save();
            float scaling = (float)keyFrameVM.PreviewSurface.Size.X / internals.Tracker.Document.Size.X;
            keyFrameVM.PreviewSurface.DrawingSurface.Canvas.Scale(scaling);
            foreach (var chunk in area.Chunks)
            {
                var pos = chunk * ChunkResolution.Full.PixelSize();
                if (!node.GetLayerImageByKeyFrameGuid(keyFrameVM.Id).DrawCommittedChunkOn(chunk, ChunkResolution.Full,
                        keyFrameVM.PreviewSurface!.DrawingSurface, pos, ReplacingPaint))
                {
                    keyFrameVM.PreviewSurface!.DrawingSurface.Canvas.DrawRect(pos.X, pos.Y, ChunkyImage.FullChunkSize,
                        ChunkyImage.FullChunkSize, ClearPaint);
                }
            }

            keyFrameVM.PreviewSurface!.DrawingSurface.Canvas.Restore();
        });*/
    }

    private void RenderMaskPreviews(List<IRenderInfo> infos)
    {
        //infos.Add(new MaskPreviewDirty_RenderInfo(guid));
    }

    private void RenderNodePreviews(List<IRenderInfo> infos)
    {
        /*using RenderingContext previewContext = new(doc.AnimationHandler.ActiveFrameTime,  VecI.Zero, ChunkResolution.Full, doc.SizeBindable);

        var outputNode = internals.Tracker.Document.NodeGraph.OutputNode;

        if (outputNode is null)
            return;

        var executionQueue = internals.Tracker.Document.NodeGraph.AllNodes; //internals.Tracker.Document.NodeGraph.CalculateExecutionQueue(outputNode);

        foreach (var node in executionQueue)
        {
            if (node is null)
                continue;

            var nodeVm = doc.StructureHelper.FindNode<INodeHandler>(node.Id);

            if (nodeVm == null)
            {
                continue;
            }

            Texture evaluated = node.Execute(previewContext);

            if (evaluated == null)
            {
                nodeVm.ResultPreview?.Dispose();
                nodeVm.ResultPreview = null;
                continue;
            }

            if (nodeVm.ResultPreview == null)
            {
                nodeVm.ResultPreview =
                    new Texture(StructureHelpers.CalculatePreviewSize(internals.Tracker.Document.Size, 150));
            }

            float scalingX = (float)nodeVm.ResultPreview.Size.X / evaluated.Size.X;
            float scalingY = (float)nodeVm.ResultPreview.Size.Y / evaluated.Size.Y;

            QueueRender(() =>
            {
                if(nodeVm.ResultPreview == null || nodeVm.ResultPreview.IsDisposed)
                    return;

                nodeVm.ResultPreview.DrawingSurface.Canvas.Save();
                nodeVm.ResultPreview.DrawingSurface.Canvas.Scale(scalingX, scalingY);

                nodeVm.ResultPreview.DrawingSurface.Canvas.DrawSurface(evaluated.DrawingSurface, 0, 0, ReplacingPaint);

                nodeVm.ResultPreview.DrawingSurface.Canvas.Restore();

                evaluated.Dispose();
            });

            infos.Add(new NodePreviewDirty_RenderInfo(node.Id));
        }*/
    }
}
