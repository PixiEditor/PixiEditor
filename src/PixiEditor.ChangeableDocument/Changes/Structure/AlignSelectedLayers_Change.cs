using ChunkyImageLib.Operations;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Objects;
using PixiEditor.ChangeableDocument.Changes.Drawing;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.ChangeableDocument.Helpers;

namespace PixiEditor.ChangeableDocument.Changes.Structure;

internal class AlignSelectedLayers_Change : Change
{
    public Guid[] MemberGuids { get; }
    public HorizontalAlignment Horizontal { get; }
    public VerticalAlignment Vertical { get; }
    public int Frame { get; set; } = 0;

    private Dictionary<Guid, Matrix3X3> oldTransforms = new();
    private Dictionary<Guid, CommittedChunkStorage> oldChunks = new();

    [GenerateMakeChangeAction]
    public AlignSelectedLayers_Change(List<Guid> memberGuids, int frame, HorizontalAlignment horizontal, VerticalAlignment vertical)
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
        List<(Guid Guid, int Index, RectD Bounds)> members = new();

        for (var i = 0; i < MemberGuids.Length; i++)
        {
            var memberGuid = MemberGuids[i];
            var member = target.FindMemberOrThrow(memberGuid);
            var bounds = member.GetTransformationCorners(Frame).AABBBounds;
            members.Add((memberGuid, i, bounds));
        }

        if (Horizontal == HorizontalAlignment.Spread)
        {
            members.Sort((a, b) => a.Bounds.Center.X.CompareTo(b.Bounds.Center.X));
        }
        else if (Vertical == VerticalAlignment.Spread)
        {
            members.Sort((a, b) => a.Bounds.Center.Y.CompareTo(b.Bounds.Center.Y));
        }

        List<IChangeInfo> changeInfos = new List<IChangeInfo>();

        for (var spatialIndex = members.Count - 1; spatialIndex >= 0; spatialIndex--)
        {
            var memberGuid = members[spatialIndex].Guid;
            var member = target.FindMemberOrThrow(memberGuid);

            if (member is ITransformableObject transformable)
            {
                VecD shift = GetAlignmentShift(member, corners.Value, spatialIndex);
                var translation = Matrix3X3.CreateTranslation(shift.X, shift.Y);
                transformable.TransformationMatrix = transformable.TransformationMatrix.PostConcat(translation);

                changeInfos.Add(new TransformObject_ChangeInfo(
                    memberGuid,
                    AffectedAreasUtility.GetTightLayerArea(member, Frame)));
            }
            else if (member is ImageLayerNode imageLayerNode)
            {
                VecD shift = GetAlignmentShift(member, corners.Value, spatialIndex);
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

   private VecD GetAlignmentShift(StructureNode node, RectD corners, int itemIndex)
{
    VecD shift = new VecD(0, 0);
    var nodeBounds = node.GetTransformationCorners(Frame).AABBBounds;

    if (Horizontal == HorizontalAlignment.Left)
    {
        shift.X = corners.Left - nodeBounds.Left;
    }
    else if (Horizontal == HorizontalAlignment.Center)
    {
        shift.X = corners.Center.X - nodeBounds.Center.X;
    }
    else if (Horizontal == HorizontalAlignment.Right)
    {
        shift.X = corners.Right - nodeBounds.Right;
    }
    else if (Horizontal == HorizontalAlignment.Spread)
    {
        int count = MemberGuids.Length;

        if (count > 1)
        {
            double firstCenter = corners.Left + nodeBounds.Width / 2;
            double lastCenter = corners.Right - nodeBounds.Width / 2;

            if (itemIndex == 0)
            {
                shift.X = firstCenter - nodeBounds.Center.X;
            }
            else if (itemIndex == count - 1)
            {
                shift.X = lastCenter - nodeBounds.Center.X;
            }
            else
            {
                double t = (double)itemIndex / (count - 1);
                double targetCenter = firstCenter + (lastCenter - firstCenter) * t;
                shift.X = targetCenter - nodeBounds.Center.X;
            }
        }
    }

    if (Vertical == VerticalAlignment.Top)
    {
        shift.Y = corners.Top - nodeBounds.Top;
    }
    else if (Vertical == VerticalAlignment.Center)
    {
        shift.Y = corners.Center.Y - nodeBounds.Center.Y;
    }
    else if (Vertical == VerticalAlignment.Bottom)
    {
        shift.Y = corners.Bottom - nodeBounds.Bottom;
    }
    else if (Vertical == VerticalAlignment.Spread)
    {
        int count = MemberGuids.Length;

        if (count > 1)
        {
            double firstCenter = corners.Top + nodeBounds.Height / 2;
            double lastCenter = corners.Bottom - nodeBounds.Height / 2;

            if (itemIndex == 0)
            {
                shift.Y = firstCenter - nodeBounds.Center.Y;
            }
            else if (itemIndex == count - 1)
            {
                shift.Y = lastCenter - nodeBounds.Center.Y;
            }
            else
            {
                double t = (double)itemIndex / (count - 1);
                double targetCenter = firstCenter + (lastCenter - firstCenter) * t;
                shift.Y = targetCenter - nodeBounds.Center.Y;
            }
        }
    }

    return shift;
}
}
