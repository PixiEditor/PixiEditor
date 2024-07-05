using System.Collections.Immutable;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.ChangeInfos.Structure;
public record class CreateFolder_ChangeInfo : CreateStructureMember_ChangeInfo
{
    public CreateFolder_ChangeInfo(
        Guid parentGuid,
        int index,
        float opacity,
        bool isVisible,
        bool clipToMemberBelow,
        string name,
        BlendMode blendMode,
        Guid guidValue,
        bool hasMask,
        bool maskIsVisible) : base(parentGuid, index, opacity, isVisible, clipToMemberBelow, name, blendMode, guidValue, hasMask, maskIsVisible)
    {
    }

    internal static CreateFolder_ChangeInfo FromFolder(Guid parentGuid, int index, FolderNode folder)
    {
        return new CreateFolder_ChangeInfo(
            parentGuid,
            index,
            folder.Opacity.Value,
            folder.IsVisible.Value,
            folder.ClipToMemberBelow.Value,
            folder.MemberName,
            folder.BlendMode.Value,
            folder.Id,
            folder.Mask.Value is not null,
            folder.MaskIsVisible.Value);
    }
}
