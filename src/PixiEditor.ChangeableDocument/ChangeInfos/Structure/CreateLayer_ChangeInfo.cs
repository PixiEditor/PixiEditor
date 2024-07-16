using System.Collections.Immutable;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.ChangeInfos.Structure;

public record class CreateLayer_ChangeInfo : CreateStructureMember_ChangeInfo
{
    public CreateLayer_ChangeInfo(
        Guid parentGuid,
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
        base(parentGuid, opacity, isVisible, clipToMemberBelow, name, blendMode, guidValue, hasMask,
            maskIsVisible, inputs, outputs)
    {
        LockTransparency = lockTransparency;
    }

    public bool LockTransparency { get; }

    internal static CreateLayer_ChangeInfo FromLayer(Guid parentGuid, LayerNode layer)
    {
        return new CreateLayer_ChangeInfo(
            parentGuid,
            layer.Opacity.Value,
            layer.IsVisible.Value,
            layer.ClipToPreviousMember.Value,
            layer.MemberName,
            layer.BlendMode.Value,
            layer.Id,
            layer.Mask.Value is not null,
            layer.MaskIsVisible.Value,
            layer is ITransparencyLockable { LockTransparency: true },
            CreatePropertyInfos(layer.InputProperties, true, layer.Id),
            CreatePropertyInfos(layer.OutputProperties, false, layer.Id)
        );
    }
}
