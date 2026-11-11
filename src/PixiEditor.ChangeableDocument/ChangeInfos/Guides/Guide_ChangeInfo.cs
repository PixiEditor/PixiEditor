namespace PixiEditor.ChangeableDocument.ChangeInfos.Guides;

public record Guide_ChangeInfo(Guid Id, bool IsVertical, double Offset) : IChangeInfo;
