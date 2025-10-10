using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph.Blackboard;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph.Blackboard;

internal class SetBlackboardVariable_Change : Change
{
    private string variable;
    private object value;

    private bool existsInBlackboard;
    private object? originalValue;

    private double min;
    private double max;
    private string unit;

    [GenerateMakeChangeAction]
    public SetBlackboardVariable_Change(string variable, object value, double min, double max, string unit)
    {
        this.variable = variable;
        this.value = value;
        this.min = min;
        this.max = max;
        this.unit = unit;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (variable == null)
            return false;

        if (target.NodeGraph.Blackboard.Variables.TryGetValue(variable, out var blackboardVariable) &&
            blackboardVariable.Type != value.GetType() && !TryConvert(blackboardVariable.Type, ref value))
            return false;

        if (blackboardVariable == null && value == null)
            return false;

        originalValue = blackboardVariable?.Value;
        existsInBlackboard = blackboardVariable != null;

        return true;
    }

    private bool TryConvert(Type targetType, ref object val)
    {
        try
        {
            if (val is IConvertible)
            {
                val = Convert.ChangeType(val, targetType);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        ignoreInUndo = false;
        if (target.NodeGraph.Blackboard.GetVariable(variable) == null)
        {
            Type type = value.GetType();
            target.NodeGraph.Blackboard.SetVariable(variable, type, value, unit, min, max);
            return new BlackboardVariable_ChangeInfo(variable, value.GetType(), value, min, max, unit);
        }

        var oldVar = target.NodeGraph.Blackboard.Variables[variable];
        target.NodeGraph.Blackboard.SetVariable(variable, oldVar.Type, value, oldVar.Unit, min, max);

        return new BlackboardVariable_ChangeInfo(variable, oldVar.Type, value, oldVar.Min ?? double.MinValue, oldVar.Max ?? double.MaxValue, oldVar.Unit);
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
        return new BlackboardVariable_ChangeInfo(variable, currentVar.Type, currentVar.Value, currentVar.Min ?? double.MinValue, currentVar.Max ?? double.MaxValue, currentVar.Unit);
    }
}
