namespace PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph.Blackboard;

public record BlackboardVariable_ChangeInfo(string Name, Type Type, object Value) : IChangeInfo;
