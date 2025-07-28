namespace PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

public record CreateNodeFrame_ChangeInfo(Guid Id, IEnumerable<Guid> NodeIds) : IChangeInfo;
