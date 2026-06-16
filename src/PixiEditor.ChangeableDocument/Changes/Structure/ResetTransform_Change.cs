using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Objects;
using PixiEditor.ChangeableDocument.Helpers;

namespace PixiEditor.ChangeableDocument.Changes.Structure;

internal class ResetTransform_Change : Change
{
    public Guid MemberGuid { get; }

    private Matrix3X3? oldTransform;

    [GenerateMakeChangeAction]
    public ResetTransform_Change(Guid memberGuid)
    {
        MemberGuid = memberGuid;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (!target.TryFindMember(MemberGuid, out var member))
            return false;

        if (member is not ITransformableObject transformable)
            return false;

        oldTransform = transformable.TransformationMatrix;

        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        return ApplyTransform(target, Matrix3X3.Identity, out ignoreInUndo);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        if (oldTransform == null)
            throw new InvalidOperationException("Old transform is not set");

        return ApplyTransform(target, oldTransform.Value, out _);
    }

    private OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTransform(Document target, Matrix3X3 matrix,
        out bool ignoreInUndo)
    {
        if (!target.TryFindMember(MemberGuid, out var member))
            throw new InvalidOperationException("Member not found");

        var area = AffectedAreasUtility.GetTightLayerArea(member, 0);
        if (member is ITransformableObject transformable)
        {
            oldTransform = transformable.TransformationMatrix;
            transformable.TransformationMatrix = matrix;
        }
        else
        {
            throw new InvalidOperationException("Member is not transformable");
        }

        area.UnionWith(AffectedAreasUtility.GetTightLayerArea(member, 0));

        ignoreInUndo = false;
        return new TransformObject_ChangeInfo(MemberGuid, area);
    }
}
