using System.Collections.Immutable;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.ChangeInfos.Structure;

public record class CreateFolder_ChangeInfo : CreateStructureMember_ChangeInfo
{
    public CreateFolder_ChangeInfo(
        string internalName,
        float opacity,
        bool isVisible,
        bool clipToMemberBelow,
        string name,
        BlendMode blendMode,
        Guid guidValue,
        bool hasMask,
        bool maskIsVisible,
        ImmutableArray<NodePropertyInfo> Inputs,
        ImmutableArray<NodePropertyInfo> Outputs,
        VecD position,
        NodeMetadata metadata
    ) : base(internalName, opacity, isVisible, clipToMemberBelow, name, blendMode, guidValue, hasMask,
        maskIsVisible, Inputs, Outputs, position, metadata)
    {
    }

    public static CreateFolder_ChangeInfo FromFolder(FolderNode folder)
    {
        return new CreateFolder_ChangeInfo(
            folder.GetNodeTypeUniqueName(),
            folder.Opacity.Value,
            folder.IsVisible.Value,
            folder.ClipToPreviousMember,
            folder.MemberName,
            folder.BlendMode.Value,
            folder.Id,
            folder.EmbeddedMask is not null,
            folder.MaskIsVisible.Value, CreatePropertyInfos(folder.InputProperties, true, folder.Id),
            CreatePropertyInfos(folder.OutputProperties, false, folder.Id),
            folder.Position,
            new NodeMetadata(folder));
    }
}
