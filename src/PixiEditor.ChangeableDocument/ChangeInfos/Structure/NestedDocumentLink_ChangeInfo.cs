namespace PixiEditor.ChangeableDocument.ChangeInfos.Structure;

public record struct NestedDocumentLink_ChangeInfo(Guid NodeId, string? OriginalFilePath, Guid ReferenceId) : IChangeInfo;

