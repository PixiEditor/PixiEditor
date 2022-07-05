using System.Collections.Immutable;
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
        bool maskIsVisible,
        ImmutableList<CreateStructureMember_ChangeInfo> children) : base(parentGuid, index, opacity, isVisible, clipToMemberBelow, name, blendMode, guidValue, hasMask, maskIsVisible)
    {
        Children = children;
    }

    public ImmutableList<CreateStructureMember_ChangeInfo> Children { get; }

    internal static CreateFolder_ChangeInfo FromFolder(Guid parentGuid, int index, Folder folder)
    {
        var builder = ImmutableList.CreateBuilder<CreateStructureMember_ChangeInfo>();
        for (int i = 0; i < folder.Children.Count; i++)
        {
            var child = folder.Children[i];
            CreateStructureMember_ChangeInfo info = child switch
            {
                Folder innerFolder => CreateFolder_ChangeInfo.FromFolder(folder.GuidValue, i, innerFolder),
                Layer innerLayer => CreateLayer_ChangeInfo.FromLayer(folder.GuidValue, i, innerLayer),
                _ => throw new NotSupportedException(),
            };
            builder.Add(info);
        }
        return new CreateFolder_ChangeInfo(
            parentGuid,
            index,
            folder.Opacity,
            folder.IsVisible,
            folder.ClipToMemberBelow,
            folder.Name,
            folder.BlendMode,
            folder.GuidValue,
            folder.Mask is not null,
            folder.MaskIsVisible,
            builder.ToImmutable()
            );
    }
}
