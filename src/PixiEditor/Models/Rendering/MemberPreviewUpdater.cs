#nullable enable

using System.Diagnostics.CodeAnalysis;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Helpers;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;
using PixiEditor.Numerics;

namespace PixiEditor.Models.Rendering;

internal class MemberPreviewUpdater
{
    private readonly IDocument doc;
    private readonly DocumentInternalParts internals;

    public MemberPreviewUpdater(IDocument doc, DocumentInternalParts internals)
    {
        this.doc = doc;
        this.internals = internals;
    }

    public void UpdatePreviews(bool rerenderPreviews, IEnumerable<Guid> membersToUpdate,
        IEnumerable<Guid> masksToUpdate, IEnumerable<Guid> nodesToUpdate)
    {
        if (!rerenderPreviews)
            return;

        UpdatePreviewPainters(membersToUpdate, masksToUpdate, nodesToUpdate);
    }

    /// <summary>
    /// Re-renders changed chunks using <see cref="mainPreviewAreasAccumulator"/> and <see cref="maskPreviewAreasAccumulator"/> along with the passed lists of bitmaps that need full re-render.
    /// </summary>
    /// <param name="members">Members that should be rendered</param>
    /// <param name="masksToUpdate">Masks that should be rendered</param>
    private void UpdatePreviewPainters(IEnumerable<Guid> members, IEnumerable<Guid> masksToUpdate, IEnumerable<Guid> nodesToUpdate)
    {
        Guid[] memberGuids = members as Guid[] ?? members.ToArray();
        Guid[] maskGuids = masksToUpdate as Guid[] ?? masksToUpdate.ToArray();
        Guid[] nodesGuids = nodesToUpdate as Guid[] ?? nodesToUpdate.ToArray();
        
        RenderWholeCanvasPreview();
        RenderMainPreviews(memberGuids);
        RenderMaskPreviews(maskGuids);
        RenderNodePreviews(nodesGuids);
    }

    /// <summary>
    /// Re-renders the preview of the whole canvas which is shown as the tab icon
    /// </summary>
    private void RenderWholeCanvasPreview()
    {
        var previewSize = StructureHelpers.CalculatePreviewSize(internals.Tracker.Document.Size);
        float scaling = (float)previewSize.X / doc.SizeBindable.X;

        if (doc.PreviewPainter == null)
        {
            doc.PreviewPainter = new PreviewPainter(doc.Renderer);
            doc.PreviewPainter.Repaint();
        }
        else
        {
            doc.PreviewPainter.Repaint();
        }
    }

    private void RenderMainPreviews(Guid[] memberGuids)
    {
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

                    structureMemberHandler.PreviewPainter =
                        new PreviewPainter(previewRenderable);
                    structureMemberHandler.PreviewPainter.Repaint();
                }
                else
                {
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

    private void RenderMaskPreviews(Guid[] members)
    {
        foreach (var node in doc.NodeGraphHandler.AllNodes)
        {
            if (node is IStructureMemberHandler structureMemberHandler)
            {
                if (!members.Contains(node.Id))
                    continue;

                var member = internals.Tracker.Document.FindMember(node.Id);
                if (member is not IPreviewRenderable previewRenderable)
                    continue;

                if (structureMemberHandler.MaskPreviewPainter == null)
                {
                    structureMemberHandler.MaskPreviewPainter = new PreviewPainter(
                        previewRenderable,
                        nameof(StructureNode.EmbeddedMask));
                    structureMemberHandler.MaskPreviewPainter.Repaint();
                }
                else
                {
                    structureMemberHandler.MaskPreviewPainter.Repaint();
                }
            }
        }
    }

    private void RenderNodePreviews(Guid[] nodesGuids)
    {
        var outputNode = internals.Tracker.Document.NodeGraph.OutputNode;

        if (outputNode is null)
            return;

        var executionQueue =
            internals.Tracker.Document.NodeGraph
                .AllNodes; //internals.Tracker.Document.NodeGraph.CalculateExecutionQueue(outputNode);

        foreach (var node in executionQueue)
        {
            if (node is null)
                continue;
            
            if (!nodesGuids.Contains(node.Id))
                continue;

            var nodeVm = doc.StructureHelper.FindNode<INodeHandler>(node.Id);

            if (nodeVm == null)
            {
                continue;
            }

            if (nodeVm.ResultPainter == null && node is IPreviewRenderable renderable)
            {
                nodeVm.ResultPainter = new PreviewPainter(renderable);
                nodeVm.ResultPainter.Repaint();
            }
            else
            {
                nodeVm.ResultPainter?.Repaint();
            }
        }
    }
}
