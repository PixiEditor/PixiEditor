#nullable enable

using System.Diagnostics.CodeAnalysis;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using PixiEditor.Helpers;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;
using Drawie.Numerics;
using PixiEditor.Parser;

namespace PixiEditor.Models.Rendering;

internal class MemberPreviewUpdater
{
    private readonly IDocument doc;
    private readonly DocumentInternalParts internals;

    private AnimationKeyFramePreviewRenderer AnimationKeyFramePreviewRenderer { get; }

    public MemberPreviewUpdater(IDocument doc, DocumentInternalParts internals)
    {
        this.doc = doc;
        this.internals = internals;
        AnimationKeyFramePreviewRenderer = new AnimationKeyFramePreviewRenderer(internals);
    }

    public void UpdatePreviews(bool rerenderPreviews, IEnumerable<Guid> membersToUpdate,
        IEnumerable<Guid> masksToUpdate, IEnumerable<Guid> nodesToUpdate, IEnumerable<Guid> keyFramesToUpdate)
    {
        if (!rerenderPreviews)
            return;

        UpdatePreviewPainters(membersToUpdate, masksToUpdate, nodesToUpdate, keyFramesToUpdate);
    }

    /// <summary>
    /// Re-renders changed chunks using <see cref="mainPreviewAreasAccumulator"/> and <see cref="maskPreviewAreasAccumulator"/> along with the passed lists of bitmaps that need full re-render.
    /// </summary>
    /// <param name="members">Members that should be rendered</param>
    /// <param name="masksToUpdate">Masks that should be rendered</param>
    private void UpdatePreviewPainters(IEnumerable<Guid> members, IEnumerable<Guid> masksToUpdate,
        IEnumerable<Guid> nodesToUpdate, IEnumerable<Guid> keyFramesToUpdate)
    {
        Guid[] memberGuids = members as Guid[] ?? members.ToArray();
        Guid[] maskGuids = masksToUpdate as Guid[] ?? masksToUpdate.ToArray();
        Guid[] nodesGuids = nodesToUpdate as Guid[] ?? nodesToUpdate.ToArray();
        Guid[] keyFramesGuids = keyFramesToUpdate as Guid[] ?? keyFramesToUpdate.ToArray();

        RenderWholeCanvasPreview();
        RenderLayersPreview(memberGuids);
        RenderMaskPreviews(maskGuids);
        RenderAnimationPreviews(memberGuids, keyFramesGuids);
        RenderNodePreviews(nodesGuids);
    }

    /// <summary>
    /// Re-renders the preview of the whole canvas which is shown as the tab icon
    /// </summary>
    private void RenderWholeCanvasPreview()
    {
        var previewSize = StructureHelpers.CalculatePreviewSize(internals.Tracker.Document.Size);
        float scaling = (float)previewSize.X / doc.SizeBindable.X;

        doc.PreviewPainter = new PreviewPainter(doc.Renderer, doc.Renderer, doc.AnimationHandler.ActiveFrameTime,
            doc.SizeBindable, internals.Tracker.Document.ProcessingColorSpace);
        doc.PreviewPainter.Repaint();
    }

    private void RenderLayersPreview(Guid[] memberGuids)
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
                        new PreviewPainter(doc.Renderer, previewRenderable,
                            doc.AnimationHandler.ActiveFrameTime, doc.SizeBindable,
                            internals.Tracker.Document.ProcessingColorSpace);
                    structureMemberHandler.PreviewPainter.Repaint();
                }
                else
                {
                    structureMemberHandler.PreviewPainter.FrameTime = doc.AnimationHandler.ActiveFrameTime;
                    structureMemberHandler.PreviewPainter.DocumentSize = doc.SizeBindable;
                    structureMemberHandler.PreviewPainter.ProcessingColorSpace =
                        internals.Tracker.Document.ProcessingColorSpace;
                    
                    structureMemberHandler.PreviewPainter.Repaint();
                }
            }
        }
    }

    private void RenderAnimationPreviews(Guid[] memberGuids, Guid[] keyFramesGuids)
    {
        foreach (var keyFrame in doc.AnimationHandler.KeyFrames)
        {
            if (keyFrame is ICelGroupHandler groupHandler)
            {
                foreach (var childFrame in groupHandler.Children)
                {
                    if (!keyFramesGuids.Contains(childFrame.Id))
                    {
                        if (!memberGuids.Contains(childFrame.LayerGuid) || !IsInFrame(childFrame))
                            continue;
                    }

                    RenderFramePreview(childFrame);
                }

                if (!memberGuids.Contains(groupHandler.LayerGuid))
                    continue;

                RenderGroupPreview(groupHandler);
            }
        }
    }

    private bool IsInFrame(ICelHandler cel)
    {
        return cel.StartFrameBindable <= doc.AnimationHandler.ActiveFrameBindable &&
               cel.StartFrameBindable + cel.DurationBindable >= doc.AnimationHandler.ActiveFrameBindable;
    }

    private void RenderFramePreview(ICelHandler cel)
    {
        if (internals.Tracker.Document.AnimationData.TryFindKeyFrame(cel.Id, out KeyFrame _))
        {
            KeyFrameTime frameTime = doc.AnimationHandler.ActiveFrameTime;
            cel.PreviewPainter = new PreviewPainter(doc.Renderer, AnimationKeyFramePreviewRenderer, frameTime, doc.SizeBindable,
                internals.Tracker.Document.ProcessingColorSpace, cel.Id.ToString());
            cel.PreviewPainter.Repaint();
        }
    }

    private void RenderGroupPreview(ICelGroupHandler groupHandler)
    {
        var group = internals.Tracker.Document.AnimationData.KeyFrames.FirstOrDefault(x => x.Id == groupHandler.Id);
        if (group != null)
        {
            KeyFrameTime frameTime = doc.AnimationHandler.ActiveFrameTime;
            ColorSpace processingColorSpace = internals.Tracker.Document.ProcessingColorSpace;
            VecI documentSize = doc.SizeBindable;

            groupHandler.PreviewPainter =
                new PreviewPainter(doc.Renderer, AnimationKeyFramePreviewRenderer, frameTime, documentSize, processingColorSpace,
                    groupHandler.Id.ToString());
            groupHandler.PreviewPainter.Repaint();
        }
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

                structureMemberHandler.MaskPreviewPainter = new PreviewPainter(
                    doc.Renderer,
                    previewRenderable,
                    doc.AnimationHandler.ActiveFrameTime,
                    doc.SizeBindable,
                    internals.Tracker.Document.ProcessingColorSpace,
                    nameof(StructureNode.EmbeddedMask));
                structureMemberHandler.MaskPreviewPainter.Repaint();
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

            if (node is IPreviewRenderable renderable)
            {
                if (nodeVm.ResultPainter == null)
                {
                    nodeVm.ResultPainter = new PreviewPainter(doc.Renderer, renderable, doc.AnimationHandler.ActiveFrameTime,
                        doc.SizeBindable, internals.Tracker.Document.ProcessingColorSpace);
                    nodeVm.ResultPainter.Repaint();
                }
                else
                {
                    nodeVm.ResultPainter.FrameTime = doc.AnimationHandler.ActiveFrameTime;
                    nodeVm.ResultPainter.DocumentSize = doc.SizeBindable;
                    nodeVm.ResultPainter.ProcessingColorSpace = internals.Tracker.Document.ProcessingColorSpace;

                    nodeVm.ResultPainter?.Repaint();
                }
            }
        }
    }
}
