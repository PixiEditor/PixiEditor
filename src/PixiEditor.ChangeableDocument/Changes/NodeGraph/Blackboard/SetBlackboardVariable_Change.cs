using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph.Blackboard;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph.Blackboard;

internal class SetBlackboardVariable_Change : Change
{
    private string variable;
    private object value;

    private bool existsInBlackboard;
    private object? originalValue;

    [GenerateMakeChangeAction]
    public SetBlackboardVariable_Change(string variable, object value)
    {
        this.variable = variable;
        this.value = value;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (variable == null)
            return false;

        if (target.NodeGraph.Blackboard.Variables.TryGetValue(variable, out var blackboardVariable) &&
            blackboardVariable.Type != value.GetType())
            return false;

        if (blackboardVariable == null && value == null)
            return false;

        originalValue = blackboardVariable?.Value;
        existsInBlackboard = blackboardVariable != null;

        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        ignoreInUndo = false;
        if (target.NodeGraph.Blackboard.GetVariable(variable) == null)
        {
            Type type = value.GetType();
            target.NodeGraph.Blackboard.SetVariable(variable, type, value);
            return new BlackboardVariable_ChangeInfo(variable, value.GetType(), value);
        }

        var oldVar = target.NodeGraph.Blackboard.Variables[variable];
        target.NodeGraph.Blackboard.SetVariable(variable, oldVar.Type, value);

        return new BlackboardVariable_ChangeInfo(variable, oldVar.Type, oldVar.Value);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        if (!existsInBlackboard)
        {
            target.NodeGraph.Blackboard.RemoveVariable(variable);
            return new BlackboardVariableRemoved_ChangeInfo(variable);
        }

        var currentVar = target.NodeGraph.Blackboard.Variables[variable];
        target.NodeGraph.Blackboard.SetVariable(variable, currentVar.Type, originalValue!);
        return new BlackboardVariable_ChangeInfo(variable, currentVar.Type, currentVar.Value);
    }
}
