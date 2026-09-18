using ChunkyImageLib.Operations;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Objects;
using PixiEditor.ChangeableDocument.Changes.Drawing;
using PixiEditor.ChangeableDocument.Helpers;

namespace PixiEditor.ChangeableDocument.Changes.Structure;

internal class CenterSelectedLayers_Change : Change
{
    public Guid[] MemberGuids { get; }
    public bool Horizontal { get; }
    public bool Vertical { get; }
    public int Frame { get; set; } = 0;

    private Dictionary<Guid, Matrix3X3> oldTransforms = new();
    private Dictionary<Guid, CommittedChunkStorage> oldChunks = new();

    [GenerateMakeChangeAction]
    public CenterSelectedLayers_Change(List<Guid> memberGuids, int frame, bool horizontal, bool vertical)
    {
        MemberGuids = memberGuids.ToArray();
        Horizontal = horizontal;
        Frame = frame;
        Vertical = vertical;
    }

    public override bool InitializeAndValidate(Document target)
    {
        foreach (Guid memberGuid in MemberGuids)
        {
            if (target.TryFindMember(memberGuid, out var member))
            {
                if (member is ITransformableObject transformable)
                {
                    oldTransforms[memberGuid] = transformable.TransformationMatrix;
                }
                else if (member is not ImageLayerNode)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        RectD? corners = null;

        foreach (var memberGuid in MemberGuids)
        {
            var member = target.FindMemberOrThrow(memberGuid);
            var tight = member.GetTightBounds(Frame);
            if (tight == null)
                continue;

            if (corners == null)
            {
                corners = tight.Value;
            }
            else
            {
                corners = corners.Value.Union(tight.Value);
            }
        }

        if (corners == null)
        {
            ignoreInUndo = true;
            return new None();
        }

        ignoreInUndo = false;
        List<IChangeInfo> changeInfos = new List<IChangeInfo>();
        var center = corners.Value.Center;
        foreach (var memberGuid in MemberGuids)
        {
            var member = target.FindMemberOrThrow(memberGuid);
            if (member is ITransformableObject transformable)
            {
                VecD memberCenter = member.GetTransformationCorners(Frame).AABBBounds.Center;
                var translation = Matrix3X3.CreateTranslation(Horizontal ? center.X - memberCenter.X : 0,
                    Vertical ? center.Y - memberCenter.Y : 0);
                transformable.TransformationMatrix = transformable.TransformationMatrix.PostConcat(translation);
                changeInfos.Add(new TransformObject_ChangeInfo(memberGuid,
                    AffectedAreasUtility.GetTightLayerArea(member, Frame)));
            }
            else if (member is ImageLayerNode imageLayerNode)
            {
                VecD shift = center - member.GetTransformationCorners(Frame).AABBBounds.Center;
                shift = new VecD(Horizontal ? shift.X : 0, Vertical ? shift.Y : 0);
                var chunks = ShiftLayerHelper.DrawShiftedLayer(target, memberGuid, false, (VecI)shift, Frame);
                changeInfos.Add(new LayerImageArea_ChangeInfo(memberGuid, chunks));
                var image = imageLayerNode.GetLayerImageAtFrame(Frame);
                oldChunks[memberGuid] = new CommittedChunkStorage(image, image.FindAffectedArea().Chunks);
                image.CommitChanges();
            }
        }

        return changeInfos;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        List<IChangeInfo> changes = new List<IChangeInfo>();
        foreach (var layerGuid in MemberGuids)
        {
            var layerNode = target.FindMemberOrThrow<LayerNode>(layerGuid);

            if (layerNode is ImageLayerNode imageNode)
            {
                var image = imageNode.GetLayerImageAtFrame(Frame);
                CommittedChunkStorage? originalChunks = oldChunks?[layerGuid];
                var affected = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(image, ref originalChunks);
                changes.Add(new LayerImageArea_ChangeInfo(layerGuid, affected));
            }
            else if (layerNode is ITransformableObject transformable)
            {
                transformable.TransformationMatrix = oldTransforms[layerGuid];

                changes.Add(new TransformObject_ChangeInfo(layerGuid,
                    AffectedAreasUtility.GetTightLayerArea(layerNode, Frame)));
            }
        }

        return changes;
    }
}
