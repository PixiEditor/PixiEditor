using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph.Blackboard;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph.Blackboard;

internal class RemoveBlackboardVariable_Change : Change
{
    private Variable originalValue;
    private readonly string variableName;

    [GenerateMakeChangeAction]
    public RemoveBlackboardVariable_Change(string variableName)
    {
        this.variableName = variableName;
    }

    public override bool InitializeAndValidate(Document target)
    {
        bool hasVar = target.NodeGraph?.Blackboard.Variables.ContainsKey(variableName) == true;
        if (hasVar)
            originalValue = target.NodeGraph.Blackboard.Variables[variableName];

        return hasVar;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        ignoreInUndo = false;

        if (target.NodeGraph?.Blackboard == null)
        {
            ignoreInUndo = true;
            return new None();
        }

        var variable = target.NodeGraph.Blackboard.GetVariable(variableName);
        if (variable == null)
        {
            ignoreInUndo = true;
            return new None();
        }

        target.NodeGraph.Blackboard.RemoveVariable(variableName);
        return new BlackboardVariableRemoved_ChangeInfo(variableName);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        if (target.NodeGraph?.Blackboard == null)
            return new None();

        target.NodeGraph.Blackboard.SetVariable(originalValue.Name, originalValue.Type, originalValue.Value, originalValue.Unit, originalValue.Min, originalValue.Max);
        return new BlackboardVariable_ChangeInfo(originalValue.Name, originalValue.Type, originalValue.Value, originalValue.Min ?? double.MinValue, originalValue.Max ?? double.MaxValue, originalValue.Unit);
    }
}
