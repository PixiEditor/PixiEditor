using System.Collections.Immutable;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.ChangeInfos.Structure;
public record class CreateLayer_ChangeInfo : CreateStructureMember_ChangeInfo
{
    public CreateLayer_ChangeInfo(
        Guid parentGuid,
        int index,
        float opacity,
        bool isVisible,
        bool clipToMemberBelow,
        string name,
        BlendMode blendMode,
        Guid guidValue,
        bool hasMask,
        bool maskIsVisible,
        bool lockTransparency,
        ImmutableArray<NodePropertyInfo> inputs,
        ImmutableArray<NodePropertyInfo> outputs) : 
        base(parentGuid, index, opacity, isVisible, clipToMemberBelow, name, blendMode, guidValue, hasMask, maskIsVisible, inputs, outputs)
    {
        LockTransparency = lockTransparency;
    }

    public bool LockTransparency { get; }

    internal static CreateLayer_ChangeInfo FromLayer(Guid parentGuid, int index, LayerNode layer)
    {
        return new CreateLayer_ChangeInfo(
            parentGuid,
            index,
            layer.Opacity.Value,
            layer.IsVisible.Value,
            layer.ClipToMemberBelow.Value,
            layer.MemberName,
            layer.BlendMode.Value,
            layer.Id,
            layer.Mask.Value is not null,
            layer.MaskIsVisible.Value,
            layer is ITransparencyLockable { LockTransparency: true }
            );
    }
}
