using System.Collections.Immutable;
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
        ImmutableArray<NodePropertyInfo> Outputs
    ) : base(internalName, opacity, isVisible, clipToMemberBelow, name, blendMode, guidValue, hasMask,
        maskIsVisible, Inputs, Outputs)
    {
    }

    internal static CreateFolder_ChangeInfo FromFolder(FolderNode folder)
    {
        return new CreateFolder_ChangeInfo(
            folder.InternalName,
            folder.Opacity.Value,
            folder.IsVisible.Value,
            folder.ClipToPreviousMember.Value,
            folder.MemberName,
            folder.BlendMode.Value,
            folder.Id,
            folder.Mask.Value is not null,
            folder.MaskIsVisible.Value, CreatePropertyInfos(folder.InputProperties, true, folder.Id),
            CreatePropertyInfos(folder.OutputProperties, false, folder.Id));
    }
}
