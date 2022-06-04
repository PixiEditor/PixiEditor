using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.ChangeInfos.Structure;

public abstract record class CreateStructureMember_ChangeInfo(
    Guid ParentGuid,
    int Index,
    float Opacity,
    bool IsVisible,
    bool ClipToMemberBelow,
    string Name,
    BlendMode BlendMode,
    Guid GuidValue,
    bool HasMask,
    bool MaskIsVisible
) : IChangeInfo;
