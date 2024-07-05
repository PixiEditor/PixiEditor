using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.ChangeInfos.Properties;
public record class StructureMemberBlendMode_ChangeInfo(Guid Id, BlendMode BlendMode) : IChangeInfo
{
}
