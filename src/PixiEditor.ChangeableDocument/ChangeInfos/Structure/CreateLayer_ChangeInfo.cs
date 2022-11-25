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
        bool lockTransparency) : base(parentGuid, index, opacity, isVisible, clipToMemberBelow, name, blendMode, guidValue, hasMask, maskIsVisible)
    {
        LockTransparency = lockTransparency;
    }

    public bool LockTransparency { get; }

    internal static CreateLayer_ChangeInfo FromLayer(Guid parentGuid, int index, Layer layer)
    {
        return new CreateLayer_ChangeInfo(
            parentGuid,
            index,
            layer.Opacity,
            layer.IsVisible,
            layer.ClipToMemberBelow,
            layer.Name,
            layer.BlendMode,
            layer.GuidValue,
            layer.Mask is not null,
            layer.MaskIsVisible,
            layer.LockTransparency
            );
    }
}
