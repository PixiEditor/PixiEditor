using ChunkyImageLib.Operations;
using Drawie.Backend.Core.Text;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.ChangeInfos.Animation;
using PixiEditor.ChangeableDocument.ChangeInfos.Properties;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;
using PixiEditor.ChangeableDocument.ChangeInfos.Vectors;
using PixiEditor.ChangeableDocument.Changes.Animation;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;
using PixiEditor.ChangeableDocument.Changes.Properties.LayerStructure;
using PixiEditor.ChangeableDocument.Changes.Structure;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.Changes.Vectors;

internal class ConvertToCurve_Change : Change
{
    public readonly Guid memberId;
    public readonly int frame;
    private ShapeVectorData originalData;
    private bool originalHighDpiRendering;
    private (VectorPath path, TextInline inline)[] paths;

    private List<CreateStructureMember_Change> createLayerMemberChanges = new();
    private DeleteStructureMember_Change? deleteLayerMemberChange;
    private CreateStructureMember_Change createFolderMemberChange;
    private List<MoveStructureMember_Change> moveLayerMemberChanges = new();
    private string nameOfLayer;
    private float opacity;
    private BlendMode blendMode;
    private bool clipToPreviousMember;
    private bool isLocked;
    private ChunkyImage? embeddedMask;

    private List<Change> executedChanges = new();
    private Guid[] celLayerIds = [];
    private Dictionary<Guid, VecD> originalPositions = new();

    [GenerateMakeChangeAction]
    public ConvertToCurve_Change(Guid memberId, int frame)
    {
        this.memberId = memberId;
        this.frame = frame;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (target.TryFindNode(memberId, out VectorLayerNode? node))
        {
            if (node.IsLocked)
            {
                return false;
            }

            var shape = node.GetShapeAtFrame(frame);

            nameOfLayer = node.MemberName;
            opacity = node.Opacity.NonOverridenValue;
            blendMode = node.BlendMode.NonOverridenValue;
            clipToPreviousMember = node.ClipToPreviousMember;
            isLocked = node.IsLocked;
            embeddedMask = node.EmbeddedMask?.CloneFromCommitted();
            if (shape is TextVectorData textVectorData && node.KeyFrames.Count == 1)
            {
                createFolderMemberChange =
                    new CreateStructureMember_Change(node.Id, Guid.NewGuid(), typeof(FolderNode));
                if (!createFolderMemberChange.InitializeAndValidate(target))
                {
                    FailedMessage = createFolderMemberChange.FailedMessage;
                    return false;
                }

                paths = GroupByPaint(textVectorData.Text.ToInlinePaths());
                for (var index = 0; index < paths.Length; index++)
                {
                    CreateStructureMember_Change createFontMemberChange = new CreateStructureMember_Change(
                        node.Id, Guid.NewGuid(), typeof(VectorLayerNode));
                    if (!createFontMemberChange.InitializeAndValidate(target))
                    {
                        FailedMessage = createFontMemberChange.FailedMessage;
                        return false;
                    }

                    createLayerMemberChanges.Add(createFontMemberChange);
                    moveLayerMemberChanges.Add(new MoveStructureMember_Change(createFontMemberChange.NewMemberGuid, createFolderMemberChange.NewMemberGuid, true));
                }

                deleteLayerMemberChange = new DeleteStructureMember_Change(node.Id);
                if (!deleteLayerMemberChange.InitializeAndValidate(target))
                {
                    FailedMessage = deleteLayerMemberChange.FailedMessage;
                    return false;
                }
            }
            else if (shape is TextVectorData celTextData)
            {
                paths = GroupByPaint(celTextData.Text.ToInlinePaths());
                foreach (var inlinePath in paths)
                {
                    inlinePath.path.Offset(celTextData.Position);
                }

                celLayerIds = new Guid[Math.Max(0, paths.Length - 1)];
                for (var index = 0; index < celLayerIds.Length; index++)
                {
                    celLayerIds[index] = Guid.NewGuid();
                }
            }

            return node.CanEditShapeAtFrame(frame) && shape != null && shape is not PathVectorData;
        }
        return false;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        VectorLayerNode node = target.FindNodeOrThrow<VectorLayerNode>(memberId);
        originalData = node.GetShapeAtFrame(frame);

        List<IChangeInfo> changeInfos = new List<IChangeInfo>();

        if (originalData is TextVectorData textVectorData)
        {
            if (paths is { Length: > 1 } && celLayerIds.Length > 0)
            {
                return SplitTextCel(target, node, textVectorData, out ignoreInUndo);
            }

            if (paths is { Length: > 1 })
            {
                for (var index = 0; index < paths.Length; index++)
                {
                    var createFontMemberChange = createLayerMemberChanges[index];
                    ApplyChange(target, createFontMemberChange, changeInfos);

                    var targetNode = target.FindNodeOrThrow<VectorLayerNode>(createFontMemberChange.NewMemberGuid);
                    string wrappedText = paths[index].inline.Text;
                    if (wrappedText.Length > 20)
                    {
                        wrappedText = wrappedText.Substring(0, 20) + "...";
                    }

                    targetNode.MemberName = $"{wrappedText}";
                    changeInfos.Add(new StructureMemberName_ChangeInfo(targetNode.Id, targetNode.MemberName));
                    var data = paths[index];
                    if (firstApply)
                    {
                        data.path.Offset(textVectorData.Position);
                    }

                    targetNode.EmbeddedShapeData = new PathVectorData(data.path)
                    {
                        Fill = data.inline.Fill,
                        FillPaintable = data.inline.FillPaintable,
                        Stroke = data.inline.StrokePaintable,
                        StrokeWidth = data.inline.StrokeWidth,
                        TransformationMatrix = textVectorData.TransformationMatrix,
                    };

                    changeInfos.Add(new VectorShape_ChangeInfo(targetNode.Id, new AffectedArea(
                        OperationHelper.FindChunksTouchingRectangle(
                            (RectI)targetNode.EmbeddedShapeData.TransformedVisualAABB, ChunkyImage.FullChunkSize))));
                }

                ApplyChange(target, createFolderMemberChange, changeInfos);

                var folder = target.FindNodeOrThrow<FolderNode>(createFolderMemberChange.NewMemberGuid);
                folder.MemberName = nameOfLayer;
                folder.Opacity.NonOverridenValue = opacity;
                folder.BlendMode.NonOverridenValue = blendMode;
                folder.ClipToPreviousMember = clipToPreviousMember;
                folder.IsLocked = isLocked;
                folder.EmbeddedMask = embeddedMask?.CloneFromCommitted();

                changeInfos.Add(new StructureMemberName_ChangeInfo(folder.Id, folder.MemberName));
                changeInfos.Add(new StructureMemberOpacity_ChangeInfo(folder.Id, opacity));
                changeInfos.Add(new StructureMemberBlendMode_ChangeInfo(folder.Id, blendMode));
                changeInfos.Add(new LayerLock_ChangeInfo(folder.Id, isLocked));
                changeInfos.Add(new StructureMemberMask_ChangeInfo(folder.Id, embeddedMask != null));

                foreach (var moveLayerMemberChange in moveLayerMemberChanges)
                {
                    ApplyChange(target, moveLayerMemberChange, changeInfos);
                }

                ApplyChange(target, deleteLayerMemberChange, changeInfos);

                ignoreInUndo = false;
                return changeInfos;
            }
        }

        // TODO: Stroke Line cap and join is missing? Validate
        var curve = new PathVectorData(originalData.ToPath())
        {
            Fill = originalData.Fill,
            FillPaintable = originalData.FillPaintable,
            Stroke = originalData.Stroke,
            StrokeWidth = originalData.StrokeWidth,
            TransformationMatrix = originalData.TransformationMatrix
        };

        if (originalData is TextVectorData && paths is { Length: 1 })
        {
            TextInline inline = paths[0].inline;
            curve.Fill = inline.Fill;
            curve.FillPaintable = inline.FillPaintable;
            curve.Stroke = inline.StrokePaintable;
            curve.StrokeWidth = inline.StrokeWidth;
        }
        node.SetShapeAtFrame(frame, curve);

        originalHighDpiRendering = node.AllowHighDpiRendering;
        node.AllowHighDpiRendering = true;

        ignoreInUndo = false;

        var aabb = node.GetShapeAtFrame(frame).TransformedVisualAABB;
        var affected = new AffectedArea(OperationHelper.FindChunksTouchingRectangle(
            (RectI)aabb, ChunkyImage.FullChunkSize));

        return new VectorShape_ChangeInfo(memberId, affected);
    }

    private static (VectorPath path, TextInline inline)[] GroupByPaint((VectorPath path, TextInline inline)[] inlinePaths)
    {
        List<(VectorPath path, TextInline inline)> groups = new();
        foreach (var inlinePath in inlinePaths)
        {
            int index = groups.FindIndex(x => HasSamePaint(x.inline, inlinePath.inline));
            if (index < 0)
            {
                groups.Add((inlinePath.path, inlinePath.inline.Clone()));
                continue;
            }

            groups[index].path.AddPath(inlinePath.path, AddPathMode.Append);
            groups[index].inline.Text += inlinePath.inline.Text;
            inlinePath.path.Dispose();
        }

        return groups.ToArray();
    }

    private static bool HasSamePaint(TextInline first, TextInline second)
    {
        return first.Fill == second.Fill
               && first.FillPaintable?.GetCacheHash() == second.FillPaintable?.GetCacheHash()
               && first.StrokePaintable?.GetCacheHash() == second.StrokePaintable?.GetCacheHash()
               && Math.Abs(first.StrokeWidth - second.StrokeWidth) < float.Epsilon;
    }

    private List<IChangeInfo> SplitTextCel(Document target, VectorLayerNode node, TextVectorData textData,
        out bool ignoreInUndo)
    {
        List<IChangeInfo> changeInfos = new List<IChangeInfo>();

        originalHighDpiRendering = node.AllowHighDpiRendering;

        for (var index = paths.Length - 1; index >= 0; index--)
        {
            var inlinePath = paths[index];
            var curve = new PathVectorData(new VectorPath(inlinePath.path))
            {
                Fill = inlinePath.inline.Fill,
                FillPaintable = inlinePath.inline.FillPaintable,
                Stroke = inlinePath.inline.StrokePaintable,
                StrokeWidth = inlinePath.inline.StrokeWidth,
                TransformationMatrix = textData.TransformationMatrix,
            };

            if (index == 0)
            {
                node.SetShapeAtFrame(frame, curve);
                node.AllowHighDpiRendering = true;
                changeInfos.Add(new VectorShape_ChangeInfo(node.Id, new AffectedArea(
                    OperationHelper.FindChunksTouchingRectangle((RectI)curve.TransformedVisualAABB,
                        ChunkyImage.FullChunkSize))));
                continue;
            }

            VectorLayerNode newNode = (VectorLayerNode)node.Clone();
            newNode.Id = celLayerIds[index - 1];

            string wrappedText = inlinePath.inline.Text;
            if (wrappedText.Length > 20)
            {
                wrappedText = wrappedText.Substring(0, 20) + "...";
            }

            newNode.MemberName = wrappedText;
            newNode.AllowHighDpiRendering = true;
            newNode.SetOnlyShapeAtFrame(frame, curve);

            target.NodeGraph.AddNode(newNode);

            changeInfos.Add(CreateLayer_ChangeInfo.FromLayer(newNode));
            changeInfos.AddRange(CreateAnimationDataFromLayer_Change.CreateTimelineRow(target, newNode));
            changeInfos.AddRange(NodeOperations.AppendMember(node, newNode, out var positions));

            foreach (var position in positions)
            {
                originalPositions[position.Key] = position.Value;
            }
        }

        ignoreInUndo = false;
        return changeInfos;
    }

    private void ApplyChange(Document target, Change change, List<IChangeInfo> changeInfos)
    {
        var result = change.Apply(target, true, out _);
        if (result.IsT1)
        {
            changeInfos.Add(result.AsT1);
        }
        else if (result.IsT2)
        {
            changeInfos.AddRange(result.AsT2);
        }

        executedChanges.Add(change);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        List<IChangeInfo> changeInfos = new List<IChangeInfo>();
        if (executedChanges.Count > 0)
        {
            var reverseChanges = executedChanges.AsEnumerable().Reverse();
            foreach (var change in reverseChanges)
            {
                var result = change.Revert(target);
                if (result.IsT1)
                {
                    changeInfos.Add(result.AsT1);
                }
                else if (result.IsT2)
                {
                    changeInfos.AddRange(result.AsT2);
                }
            }
            executedChanges.Clear();
        }
        else
        {
            VectorLayerNode node = target.FindNodeOrThrow<VectorLayerNode>(memberId);
            node.SetShapeAtFrame(frame, originalData);

            node.AllowHighDpiRendering = originalHighDpiRendering;

            var aabb = originalData.TransformedVisualAABB;
            var affected = new AffectedArea(OperationHelper.FindChunksTouchingRectangle(
                (RectI)aabb, ChunkyImage.FullChunkSize));

            changeInfos.Add(new VectorShape_ChangeInfo(memberId, affected));

            changeInfos.AddRange(NodeOperations.RevertPositions(originalPositions, target));
            originalPositions.Clear();

            foreach (var celLayerId in celLayerIds)
            {
                var newNode = target.FindNode<VectorLayerNode>(celLayerId);
                if (newNode == null)
                    continue;

                if (target.AnimationData.TryFindKeyFrame<GroupKeyFrame>(celLayerId, out _))
                {
                    target.AnimationData.RemoveKeyFrame(celLayerId);
                    changeInfos.Add(new DeleteKeyFrame_ChangeInfo(celLayerId));
                }

                changeInfos.AddRange(NodeOperations.DetachStructureNode(newNode));
                changeInfos.Add(new DeleteStructureMember_ChangeInfo(celLayerId));
                target.NodeGraph.RemoveNode(newNode);
            }
        }
        return changeInfos;
    }

    public override void Dispose()
    {
        embeddedMask?.Dispose();
    }
}
