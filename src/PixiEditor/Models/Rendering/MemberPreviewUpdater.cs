#nullable enable

using System.Diagnostics.CodeAnalysis;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Changeables.Animations;
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
    private void UpdatePreviewPainters(IEnumerable<Guid> members, IEnumerable<Guid> masksToUpdate,
        IEnumerable<Guid> nodesToUpdate)
    {
        Guid[] memberGuids = members as Guid[] ?? members.ToArray();
        Guid[] maskGuids = masksToUpdate as Guid[] ?? masksToUpdate.ToArray();
        Guid[] nodesGuids = nodesToUpdate as Guid[] ?? nodesToUpdate.ToArray();

        RenderWholeCanvasPreview();
        RenderLayersPreview(memberGuids);
        RenderMaskPreviews(maskGuids);
        RenderAnimationPreviews(memberGuids);
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

    private void RenderAnimationPreviews(Guid[] memberGuids)
    {
        foreach (var keyFrame in doc.AnimationHandler.KeyFrames)
        {
            if (keyFrame is IKeyFrameGroupHandler groupHandler)
            {
                foreach (var childFrame in groupHandler.Children)
                {
                    if (!memberGuids.Contains(childFrame.LayerGuid) || !IsInFrame(childFrame))
                        continue;

                    RenderFramePreview(childFrame);
                }

                if (!memberGuids.Contains(groupHandler.LayerGuid))
                    continue;

                RenderGroupPreview(groupHandler);
            }
        }
    }
    
    private bool IsInFrame(IKeyFrameHandler keyFrame)
    {
        return keyFrame.StartFrameBindable <= doc.AnimationHandler.ActiveFrameBindable &&
               keyFrame.StartFrameBindable + keyFrame.DurationBindable >= doc.AnimationHandler.ActiveFrameBindable;
    }

    private void RenderFramePreview(IKeyFrameHandler keyFrame)
    {
        if (internals.Tracker.Document.AnimationData.TryFindKeyFrame(keyFrame.Id, out KeyFrame foundKeyFrame))
        {
            keyFrame.PreviewPainter ??= new PreviewPainter(AnimationKeyFramePreviewRenderer, keyFrame.Id.ToString());
            keyFrame.PreviewPainter.Repaint();
        }
    }
    
    private void RenderGroupPreview(IKeyFrameGroupHandler groupHandler)
    {
        var group = internals.Tracker.Document.AnimationData.KeyFrames.FirstOrDefault(x => x.Id == groupHandler.Id);
        if (group != null)
        {
            groupHandler.PreviewPainter ??= new PreviewPainter(AnimationKeyFramePreviewRenderer, groupHandler.Id.ToString());
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
