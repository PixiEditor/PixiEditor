using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph.Blackboard;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph.Blackboard;

internal class SetBlackboardVariable_Change : Change
{
    private object value;
    private string variable;

    [GenerateMakeChangeAction]
    public SetBlackboardVariable_Change(string variable, object value)
    {
        this.variable = variable;
        this.value = value;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (!target.NodeGraph.Blackboard.Variables.ContainsKey(variable))
            return false;

        if (target.NodeGraph.Blackboard.Variables[variable].Type != value.GetType())
            return false;

        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        var oldVar = target.NodeGraph.Blackboard.Variables[variable];
        target.NodeGraph.Blackboard.SetVariable(variable, oldVar.Type, value);
        ignoreInUndo = false;

        return new BlackboardVariable_ChangeInfo(variable, oldVar.Type, oldVar.Value);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        throw new NotImplementedException();
    }
}
