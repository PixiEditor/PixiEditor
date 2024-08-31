namespace PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

public record UpdateConstantValue_ChangeInfo(Guid Id, object Value) : IChangeInfo;
