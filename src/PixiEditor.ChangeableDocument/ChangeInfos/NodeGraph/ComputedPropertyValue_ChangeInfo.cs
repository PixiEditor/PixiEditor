namespace PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

public record ComputedPropertyValue_ChangeInfo(Guid Node, string PropertyName, bool IsInput, object? Value)
    : IChangeInfo;
