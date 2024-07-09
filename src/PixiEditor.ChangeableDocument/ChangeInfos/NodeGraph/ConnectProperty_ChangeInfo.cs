namespace PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

public record ConnectProperty_ChangeInfo(
    Guid? SourceNodeId,
    Guid TargetNodeId,
    string? SourceProperty,
    string TargetProperty) : IChangeInfo;
