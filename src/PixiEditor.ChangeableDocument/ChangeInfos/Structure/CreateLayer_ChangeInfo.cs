using System.Collections.Immutable;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.ChangeInfos.Structure;

public record class CreateLayer_ChangeInfo : CreateStructureMember_ChangeInfo
{
    public CreateLayer_ChangeInfo(
        string internalName,
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
        ImmutableArray<NodePropertyInfo> outputs,
        VecD position,
        NodeMetadata metadata) :
        base(internalName, opacity, isVisible, clipToMemberBelow, name, blendMode, guidValue, hasMask,
            maskIsVisible, inputs, outputs, position, metadata)
    {
        LockTransparency = lockTransparency;
    }

    public bool LockTransparency { get; }

    public static CreateLayer_ChangeInfo FromLayer(LayerNode layer)
    {
        return new CreateLayer_ChangeInfo(
            layer.GetNodeTypeUniqueName(),
            layer.Opacity.Value,
            layer.IsVisible.Value,
            layer.ClipToPreviousMember,
            layer.MemberName,
            layer.BlendMode.Value,
            layer.Id,
            layer.EmbeddedMask is not null,
            layer.MaskIsVisible.Value,
            layer is ITransparencyLockable { LockTransparency: true },
            CreatePropertyInfos(layer.InputProperties, true, layer.Id),
            CreatePropertyInfos(layer.OutputProperties, false, layer.Id),
            layer.Position,
            new NodeMetadata(layer.GetType())
        );
    }
}
