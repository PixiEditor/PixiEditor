using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph.Blackboard;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph.Blackboard;

internal class SetBlackboardVariable_Change : InterruptableUpdateableChange
{
    private string variable;
    private object value;
    private Type type;

    private bool existsInBlackboard;
    private object? originalValue;

    private double min;
    private double max;
    private string unit;
    private bool isExposed;


    [GenerateUpdateableChangeActions]
    public SetBlackboardVariable_Change(string variable, object value, Type type, double min, double max, string unit,
        bool isExposed)
    {
        this.variable = variable;
        this.value = value;
        this.type = type;
        this.min = min;
        this.max = max;
        this.unit = unit;
        this.isExposed = isExposed;
    }

    [UpdateChangeMethod]
    public void Update(string variable, object value, double min, double max, string unit, bool isExposed)
    {
        this.variable = variable;
        this.value = value;
        this.min = min;
        this.max = max;
        this.unit = unit;
        this.isExposed = isExposed;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (variable == null)
            return false;

        if (target?.NodeGraph?.Blackboard?.Variables == null || string.IsNullOrEmpty(variable))
            return false;

        if (target.NodeGraph.Blackboard.Variables.TryGetValue(variable, out var blackboardVariable) &&
            !IsTypeAssignable(blackboardVariable) && !TryConvert(blackboardVariable?.Type, ref value))
            return false;

        originalValue = blackboardVariable?.Value;
        existsInBlackboard = blackboardVariable != null;

        return true;
    }

    private bool IsTypeAssignable(Variable blackboardVariable)
    {
        if (blackboardVariable?.Type == null || value == null)
            return false;

        return value.GetType().IsAssignableTo(blackboardVariable.Type);
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
        return Apply(target);
    }

    private OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target)
    {
        if (target.NodeGraph.Blackboard.GetVariable(variable) == null)
        {
            target.NodeGraph.Blackboard.SetVariable(variable, type, value, unit, min, max, isExposed);
            InformBlackboardAccessingNodes(target, variable);
            return new List<IChangeInfo>()
            {
                new BlackboardVariable_ChangeInfo(variable, type, value, min, max, unit),
                new BlackboardVariableExposed_ChangeInfo(variable, isExposed)
            };
        }

        var oldVar = target.NodeGraph.Blackboard.Variables.GetValueOrDefault(variable);

        target.NodeGraph.Blackboard.SetVariable(variable, oldVar?.Type, value, oldVar?.Unit, min, max, isExposed);

        InformBlackboardAccessingNodes(target, variable);
        return new List<IChangeInfo>()
        {
            new BlackboardVariable_ChangeInfo(variable, oldVar?.Type, value, oldVar?.Min ?? double.MinValue,
                oldVar?.Max ?? double.MaxValue, oldVar?.Unit),
            new BlackboardVariableExposed_ChangeInfo(variable, isExposed)
        };
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        return Apply(target);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        if (!existsInBlackboard)
        {
            target.NodeGraph.Blackboard.RemoveVariable(variable);
            InformBlackboardAccessingNodes(target, variable);
            return new BlackboardVariableRemoved_ChangeInfo(variable);
        }

        var currentVar = target.NodeGraph.Blackboard.Variables.GetValueOrDefault(variable);
        target.NodeGraph.Blackboard.SetVariable(variable, currentVar?.Type, originalValue!);
        InformBlackboardAccessingNodes(target, variable);
        return new List<IChangeInfo>()
        {
            new BlackboardVariable_ChangeInfo(variable, currentVar?.Type, currentVar?.Value,
                currentVar?.Min ?? double.MinValue, currentVar?.Max ?? double.MaxValue, currentVar?.Unit),
            new BlackboardVariableExposed_ChangeInfo(variable, currentVar?.IsExposed ?? false)
        };
    }

    private void InformBlackboardAccessingNodes(Document target, string variableName)
    {
        foreach (var node in target.NodeGraph.Nodes.OfType<BlackboardVariableValueNode>())
        {
            if (node.VariableName.Value == variableName)
            {
                node.UpdateValuesFromBlackboard(target.NodeGraph.Blackboard);
            }
        }
    }
}
