using ChunkyImageLib.Operations;
using Drawie.Backend.Core.Text;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.ChangeInfos.Properties;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;
using PixiEditor.ChangeableDocument.ChangeInfos.Vectors;
using PixiEditor.ChangeableDocument.Changes.Structure;

namespace PixiEditor.ChangeableDocument.Changes.Vectors;

internal class ConvertToCurve_Change : Change
{
    public readonly Guid memberId;

    private ShapeVectorData originalData;
    private bool originalHighDpiRendering;
    private (VectorPath path, TextInline inline)[] paths;

    private List<CreateStructureMember_Change> createLayerMemberChanges = new();
    private DeleteStructureMember_Change? deleteLayerMemberChange;
    private CreateStructureMember_Change createFolderMemberChange;
    private List<MoveStructureMember_Change> moveLayerMemberChanges = new();
    private string nameOfLayer;

    private List<Change> executedChanges = new();

    [GenerateMakeChangeAction]
    public ConvertToCurve_Change(Guid memberId)
    {
        this.memberId = memberId;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (target.TryFindNode(memberId, out VectorLayerNode? node))
        {
            nameOfLayer = node.MemberName;
            if (node.EmbeddedShapeData is TextVectorData textVectorData)
            {
                createFolderMemberChange =
                    new CreateStructureMember_Change(node.Id, Guid.NewGuid(), typeof(FolderNode));
                if (!createFolderMemberChange.InitializeAndValidate(target))
                {
                    FailedMessage = createFolderMemberChange.FailedMessage;
                    return false;
                }

                paths = textVectorData.Text.ToInlinePaths();
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

            return node.EmbeddedShapeData != null && node.EmbeddedShapeData is not PathVectorData;
        }

        return false;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        VectorLayerNode node = target.FindNodeOrThrow<VectorLayerNode>(memberId);
        originalData = node.EmbeddedShapeData;

        List<IChangeInfo> changeInfos = new List<IChangeInfo>();

        if (originalData is TextVectorData textVectorData)
        {
            if (paths.Length > 1)
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
                        TransformationMatrix = textVectorData.TransformationMatrix
                    };

                    changeInfos.Add(new VectorShape_ChangeInfo(targetNode.Id, new AffectedArea(
                        OperationHelper.FindChunksTouchingRectangle(
                            (RectI)targetNode.EmbeddedShapeData.TransformedVisualAABB, ChunkyImage.FullChunkSize))));
                }

                ApplyChange(target, createFolderMemberChange, changeInfos);

                var folder = target.FindNodeOrThrow<FolderNode>(createFolderMemberChange.NewMemberGuid);
                folder.MemberName = nameOfLayer;
                changeInfos.Add(new StructureMemberName_ChangeInfo(folder.Id, folder.MemberName));

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
        node.EmbeddedShapeData = new PathVectorData(originalData.ToPath())
        {
            Fill = originalData.Fill,
            FillPaintable = originalData.FillPaintable,
            Stroke = originalData.Stroke,
            StrokeWidth = originalData.StrokeWidth,
            TransformationMatrix = originalData.TransformationMatrix
        };

        originalHighDpiRendering = node.AllowHighDpiRendering;
        node.AllowHighDpiRendering = true;

        ignoreInUndo = false;

        var aabb = node.EmbeddedShapeData.TransformedVisualAABB;
        var affected = new AffectedArea(OperationHelper.FindChunksTouchingRectangle(
            (RectI)aabb, ChunkyImage.FullChunkSize));

        return new VectorShape_ChangeInfo(memberId, affected);
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
            node.EmbeddedShapeData = originalData;

            node.AllowHighDpiRendering = originalHighDpiRendering;

            var aabb = node.EmbeddedShapeData.TransformedVisualAABB;
            var affected = new AffectedArea(OperationHelper.FindChunksTouchingRectangle(
                (RectI)aabb, ChunkyImage.FullChunkSize));

            changeInfos.Add(new VectorShape_ChangeInfo(memberId, affected));
        }
        return changeInfos;
    }
}
