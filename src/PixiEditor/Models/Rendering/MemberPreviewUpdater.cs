#nullable enable

using Drawie.Backend.Core;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ViewModels.Nodes;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.Models.DocumentPassthroughActions;
using PixiEditor.ViewModels.Document;

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

    public Dictionary<Guid, List<PreviewRenderRequest>> GatherPreviewsToUpdate(HashSet<Guid> membersToUpdate,
        HashSet<Guid> masksToUpdate, HashSet<Guid> nodesToUpdate, HashSet<Guid> keyFramesToUpdate,
        bool ignoreAnimationPreviews, bool renderMiniPreviews)
    {
        if (!membersToUpdate.Any() && !masksToUpdate.Any() && !nodesToUpdate.Any() &&
            !keyFramesToUpdate.Any())
            return null;

        return UpdatePreviewPainters(membersToUpdate, masksToUpdate, nodesToUpdate, keyFramesToUpdate,
            ignoreAnimationPreviews,
            renderMiniPreviews);
    }

    /// <summary>
    /// Re-renders changed chunks using <see cref="mainPreviewAreasAccumulator"/> and <see cref="maskPreviewAreasAccumulator"/> along with the passed lists of bitmaps that need full re-render.
    /// </summary>
    /// <param name="members">Members that should be rendered</param>
    /// <param name="masksToUpdate">Masks that should be rendered</param>
    private Dictionary<Guid, List<PreviewRenderRequest>>? UpdatePreviewPainters(HashSet<Guid> members,
        HashSet<Guid> masksToUpdate,
        HashSet<Guid> nodesToUpdate, HashSet<Guid> keyFramesToUpdate, bool ignoreAnimationPreviews,
        bool renderLowPriorityPreviews)
    {
        Dictionary<Guid, List<PreviewRenderRequest>> previewTextures = new();
        if (renderLowPriorityPreviews)
        {
            RenderLayersPreview(members, previewTextures);
            RenderMaskPreviews(masksToUpdate, previewTextures);
        }

        if (!ignoreAnimationPreviews)
        {
            RenderAnimationPreviews(members, keyFramesToUpdate, previewTextures);
        }

        RenderNodePreviews(nodesToUpdate, previewTextures);

        return previewTextures;
    }

    /// <summary>
    /// Re-renders the preview of the whole canvas which is shown as the tab icon
    /// </summary>
    /// <param name="memberGuids"></param>
    /// <param name="previewTextures"></param>
    /// <param name="renderMiniPreviews">Decides whether to re-render mini previews for the document</param>
    /*private void RenderWholeCanvasPreview(bool renderMiniPreviews)
    {
        var previewSize = StructureHelpers.CalculatePreviewSize(internals.Tracker.Document.Size);
        //float scaling = (float)previewSize.X / doc.SizeBindable.X;

        doc.PreviewPainter ??= new PreviewPainter(renderer, doc.Renderer, doc.AnimationHandler.ActiveFrameTime,
            doc.SizeBindable, internals.Tracker.Document.ProcessingColorSpace);

        UpdateDocPreviewPainter(doc.PreviewPainter);

        if (!renderMiniPreviews)
            return;

        doc.MiniPreviewPainter ??= new PreviewPainter(renderer, doc.Renderer,
            doc.AnimationHandler.ActiveFrameTime,
            doc.SizeBindable, internals.Tracker.Document.ProcessingColorSpace);

        UpdateDocPreviewPainter(doc.MiniPreviewPainter);
    }

    private void UpdateDocPreviewPainter(PreviewPainter painter)
    {
        painter.DocumentSize = doc.SizeBindable;
        painter.ProcessingColorSpace = internals.Tracker.Document.ProcessingColorSpace;
        painter.FrameTime = doc.AnimationHandler.ActiveFrameTime;
        painter.Repaint();
    }*/

    private void RenderLayersPreview(HashSet<Guid> memberGuids,
        Dictionary<Guid, List<PreviewRenderRequest>> previewTextures)
    {
        foreach (var node in doc.NodeGraphHandler.AllNodes)
        {
            if (node is IStructureMemberHandler structureMemberHandler)
            {
                if (!memberGuids.Contains(node.Id))
                    continue;

                var member = internals.Tracker.Document.FindMember(node.Id);
                if (structureMemberHandler.Preview == null)
                {
                    structureMemberHandler.Preview = new TexturePreview(node.Id, RequestRender);
                    continue;
                }

                if (structureMemberHandler.Preview.Listeners.Count == 0)
                {
                    structureMemberHandler.Preview.Preview?.Dispose();
                    continue;
                }

                if (!previewTextures.ContainsKey(node.Id))
                    previewTextures[node.Id] = new List<PreviewRenderRequest>();

                VecI textureSize = structureMemberHandler.Preview.GetMaxListenerSize();
                if (textureSize.X <= 0 || textureSize.Y <= 0)
                    continue;

                Texture? CreateTextureForLayer(bool createIfNull)
                {
                    if (createIfNull)
                    {
                        if (structureMemberHandler.Preview.Preview == null ||
                            structureMemberHandler.Preview.Preview.IsDisposed ||
                            structureMemberHandler.Preview.Preview.Size != textureSize)
                        {
                            structureMemberHandler.Preview.Preview?.Dispose();
                            structureMemberHandler.Preview.Preview = Texture.ForDisplay(textureSize);
                        }
                    }

                    return structureMemberHandler.Preview.Preview;
                }

                previewTextures[node.Id].Add(new PreviewRenderRequest(CreateTextureForLayer,
                    structureMemberHandler.Preview.InvokeTextureUpdated));
            }
        }
    }

    private void RenderAnimationPreviews(HashSet<Guid> memberGuids, HashSet<Guid> keyFramesGuids,
        Dictionary<Guid, List<PreviewRenderRequest>> previewTextures)
    {
        foreach (var keyFrame in doc.AnimationHandler.KeyFrames)
        {
            if (keyFrame is ICelGroupHandler groupHandler)
            {
                foreach (var childFrame in groupHandler.Children)
                {
                    if (!keyFramesGuids.Contains(childFrame.Id))
                    {
                        if (!memberGuids.Contains(childFrame.LayerGuid) || !IsInFrame(childFrame) ||
                            !groupHandler.IsVisible)
                            continue;
                    }

                    RenderFramePreview(childFrame, previewTextures);
                }

                if (!keyFramesGuids.Contains(groupHandler.LayerGuid) && !memberGuids.Contains(groupHandler.LayerGuid))
                    continue;

                RenderGroupPreview(groupHandler, previewTextures);
            }
        }
    }

    private bool IsInFrame(ICelHandler cel)
    {
        return cel.StartFrameBindable <= doc.AnimationHandler.ActiveFrameBindable &&
               cel.StartFrameBindable + cel.DurationBindable > doc.AnimationHandler.ActiveFrameBindable;
    }

    private void RenderFramePreview(ICelHandler cel, Dictionary<Guid, List<PreviewRenderRequest>> previewTextures)
    {
        if (internals.Tracker.Document.AnimationData.TryFindKeyFrame(cel.Id, out KeyFrame _))
        {
            if (cel.PreviewTexture == null)
            {
                cel.PreviewTexture = new TexturePreview(cel.LayerGuid, cel.Id, RequestCelRender);
                return;
            }

            if (cel.PreviewTexture.Listeners.Count == 0)
            {
                cel.PreviewTexture.Preview?.Dispose();
                return;
            }

            VecI textureSize = cel.PreviewTexture.GetMaxListenerSize();
            if (textureSize.X <= 0 || textureSize.Y <= 0)
                return;

            Texture? CreateTextureForCel(bool createIfNull)
            {
                if (createIfNull)
                {
                    if (cel.PreviewTexture.Preview == null || cel.PreviewTexture.Preview.IsDisposed ||
                        cel.PreviewTexture.Preview.Size != textureSize)
                    {
                        cel.PreviewTexture.Preview?.Dispose();
                        cel.PreviewTexture.Preview = Texture.ForDisplay(textureSize);
                    }
                }

                return cel.PreviewTexture.Preview;
            }

            if (!previewTextures.ContainsKey(cel.LayerGuid))
                previewTextures[cel.LayerGuid] = new List<PreviewRenderRequest>();

            previewTextures[cel.LayerGuid].Add(new PreviewRenderRequest(CreateTextureForCel,
                cel.PreviewTexture.InvokeTextureUpdated, cel.Id.ToString()));
        }
    }

    private void RenderGroupPreview(ICelGroupHandler groupHandler,
        Dictionary<Guid, List<PreviewRenderRequest>> previewTextures)
    {
        var group = internals.Tracker.Document.AnimationData.KeyFrames.FirstOrDefault(x => x.Id == groupHandler.Id);
        if (group != null)
        {
            if (groupHandler.PreviewTexture == null)
            {
                groupHandler.PreviewTexture = new TexturePreview(groupHandler.LayerGuid, groupHandler.LayerGuid, RequestCelRender);
                return;
            }

            if (groupHandler.PreviewTexture.Listeners.Count == 0)
            {
                groupHandler.PreviewTexture.Preview?.Dispose();
                return;
            }

            VecI textureSize = groupHandler.PreviewTexture.GetMaxListenerSize();
            if (textureSize.X <= 0 || textureSize.Y <= 0)
                return;

            Texture? CreateTextureForGroup(bool createIfNull)
            {
                if (createIfNull)
                {
                    if (groupHandler.PreviewTexture.Preview == null ||
                        groupHandler.PreviewTexture.Preview.IsDisposed ||
                        groupHandler.PreviewTexture.Preview.Size != textureSize)
                    {
                        groupHandler.PreviewTexture.Preview?.Dispose();
                        groupHandler.PreviewTexture.Preview = Texture.ForDisplay(textureSize);
                    }
                }

                return groupHandler.PreviewTexture.Preview;
            }

            if (!previewTextures.ContainsKey(groupHandler.LayerGuid))
                previewTextures[groupHandler.LayerGuid] = new List<PreviewRenderRequest>();

            previewTextures[groupHandler.LayerGuid].Add(new PreviewRenderRequest(CreateTextureForGroup,
                groupHandler.PreviewTexture.InvokeTextureUpdated, groupHandler.Id.ToString()));
        }
    }

    private void RenderMaskPreviews(HashSet<Guid> members,
        Dictionary<Guid, List<PreviewRenderRequest>> previewTextures)
    {
        foreach (var node in doc.NodeGraphHandler.AllNodes)
        {
            if (node is IStructureMemberHandler structureMemberHandler)
            {
                if (!members.Contains(node.Id))
                    continue;

                if (structureMemberHandler.MaskPreview == null)
                {
                    structureMemberHandler.MaskPreview = new TexturePreview(node.Id, RequestMaskRender);
                    continue;
                }

                if (structureMemberHandler.MaskPreview.Listeners.Count == 0)
                {
                    structureMemberHandler.MaskPreview.Preview?.Dispose();
                    continue;
                }

                if (!previewTextures.ContainsKey(node.Id))
                    previewTextures[node.Id] = new List<PreviewRenderRequest>();

                VecI textureSize = structureMemberHandler.MaskPreview.GetMaxListenerSize();
                if (textureSize.X <= 0 || textureSize.Y <= 0)
                    continue;

                Texture? CreateTextureForMask(bool createIfNull)
                {
                    if (createIfNull)
                    {
                        if (structureMemberHandler.MaskPreview.Preview == null ||
                            structureMemberHandler.MaskPreview.Preview.IsDisposed ||
                            structureMemberHandler.MaskPreview.Preview.Size != textureSize)
                        {
                            structureMemberHandler.MaskPreview.Preview?.Dispose();
                            structureMemberHandler.MaskPreview.Preview = Texture.ForDisplay(textureSize);
                        }
                    }

                    return structureMemberHandler.MaskPreview.Preview;
                }

                previewTextures[node.Id].Add(new PreviewRenderRequest(CreateTextureForMask,
                    structureMemberHandler.MaskPreview.InvokeTextureUpdated) {ElementToRender = nameof(StructureNode.EmbeddedMask) });
            }
        }
    }

    private void RenderNodePreviews(HashSet<Guid> nodesGuids,
        Dictionary<Guid, List<PreviewRenderRequest>>? previews = null)
    {
        var outputNode = internals.Tracker.Document.NodeGraph.OutputNode;

        if (outputNode is null)
            return;

        var allNodes =
            internals.Tracker.Document.NodeGraph
                .AllNodes; //internals.Tracker.Document.NodeGraph.CalculateExecutionQueue(outputNode);

        if (nodesGuids.Count == 0)
            return;

        List<Guid> actualRepaintedNodes = new();
        foreach (var guid in nodesGuids)
        {
            QueueRepaintNode(actualRepaintedNodes, guid, allNodes, previews);
        }
    }

    private bool IsSmallStructureNode(Guid guid, Dictionary<Guid, List<PreviewRenderRequest>>? previews)
    {
        var node = doc.StructureHelper.FindNode<INodeHandler>(guid);
        if (node is null)
            return false;

        if (node is not IStructureMemberHandler memberHandler)
            return false;

        var member = internals.Tracker.Document.FindMember(guid);
        if (member is null)
            return false;

        if(previews == null || !previews.ContainsKey(member.Id))
            return false;

        var value = previews[guid];

        return value is { Count: > 0 } && value.Max(x => x.Texture.Size.LongestAxis < 64);
    }

    private void QueueRepaintNode(List<Guid> actualRepaintedNodes, Guid guid,
        IReadOnlyCollection<IReadOnlyNode> allNodes, Dictionary<Guid, List<PreviewRenderRequest>>? previews)
    {
        if (actualRepaintedNodes.Contains(guid))
            return;

        var nodeVm = doc.StructureHelper.FindNode<INodeHandler>(guid);
        if (nodeVm == null)
        {
            return;
        }

        actualRepaintedNodes.Add(guid);
        IReadOnlyNode node = allNodes.FirstOrDefault(x => x.Id == guid);
        if (node is null)
            return;

        RequestRepaintNode(node, nodeVm, previews);

        nodeVm.TraverseForwards(next =>
        {
            if (next is not INodeHandler nextVm)
                return Traverse.Further;

            var nextNode = allNodes.FirstOrDefault(x => x.Id == next.Id);

            if (nextNode is null || actualRepaintedNodes.Contains(next.Id))
                return Traverse.Further;

            RequestRepaintNode(nextNode, nextVm, previews);
            actualRepaintedNodes.Add(next.Id);
            return Traverse.Further;
        });
    }

    private void RequestRepaintNode(IReadOnlyNode node, INodeHandler nodeVm,
        Dictionary<Guid, List<PreviewRenderRequest>>? previews)
    {
        if (previews == null)
            return;

        nodeVm.Preview ??= new TexturePreview(node.Id, RequestRender);
        if (nodeVm.Preview.Listeners.Count == 0)
        {
            nodeVm.Preview.Preview?.Dispose();
            return;
        }

        if (!previews.ContainsKey(node.Id))
            previews[node.Id] = new List<PreviewRenderRequest>();

        if (previews.TryGetValue(node.Id, out var existingPreviews) &&
            existingPreviews.Any(x => string.IsNullOrEmpty(x.ElementToRender)))
            return;

        VecI textureSize = nodeVm.Preview.GetMaxListenerSize();
        if (nodeVm is IStructureMemberHandler && textureSize.LongestAxis < 64)
            return;
        if (textureSize.X <= 0 || textureSize.Y <= 0)
            return;

        Texture? CreateTextureForNode(bool createIfNull)
        {
            if (createIfNull)
            {
                if (nodeVm.Preview.Preview == null || nodeVm.Preview.Preview.IsDisposed ||
                    nodeVm.Preview.Preview.Size != textureSize)
                {
                    nodeVm.Preview.Preview?.Dispose();
                    nodeVm.Preview.Preview = Texture.ForDisplay(textureSize);
                }
            }

            return nodeVm.Preview.Preview;
        };

        previews[node.Id]
            .Add(new PreviewRenderRequest(CreateTextureForNode, nodeVm.Preview.InvokeTextureUpdated));
    }

    private void RequestRender(Guid id)
    {
        internals.ActionAccumulator.AddActions(new RefreshPreview_PassthroughAction(id));
    }

    private void RequestCelRender(Guid nodeId, Guid? celId)
    {
        internals.ActionAccumulator.AddActions(new RefreshPreview_PassthroughAction(nodeId, celId));
    }

    private void RequestMaskRender(Guid id)
    {
        internals.ActionAccumulator.AddActions(new RefreshPreview_PassthroughAction(id, null, nameof(StructureNode.EmbeddedMask)));
    }
}
