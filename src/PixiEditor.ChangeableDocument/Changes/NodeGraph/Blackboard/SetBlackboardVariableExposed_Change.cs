using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph.Blackboard;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph.Blackboard;

internal class SetBlackboardVariableExposed_Change : Change
{
    private readonly string variableName;
    private readonly bool isExposed;
    private bool wasExposed;

    [GenerateMakeChangeAction]
    public SetBlackboardVariableExposed_Change(string variableName, bool isExposed)
    {
        this.variableName = variableName;
        this.isExposed = isExposed;
    }

    public override bool InitializeAndValidate(Document target)
    {
        var variable = target.NodeGraph.Blackboard.Variables.FirstOrDefault(v => v.Key == variableName);
        if (variable.Value == null)
            return false;

        wasExposed = variable.Value.IsExposed;
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        var variable = target.NodeGraph.Blackboard.Variables[variableName];
        variable.IsExposed = isExposed;
        ignoreInUndo = false;
        return new BlackboardVariableExposed_ChangeInfo(variableName, isExposed);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var variable = target.NodeGraph.Blackboard.Variables[variableName];
        variable.IsExposed = wasExposed;
        return new BlackboardVariableExposed_ChangeInfo(variableName, wasExposed);
    }
}
