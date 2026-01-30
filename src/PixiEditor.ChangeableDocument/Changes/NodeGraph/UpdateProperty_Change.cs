using Drawie.Backend.Core.Shaders.Generation;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

internal class UpdatePropertyValue_Change : InterruptableUpdateableChange
{
    private readonly Guid _nodeId;
    private readonly string _propertyName;
    private object? _value;
    private object? previousValue;

    [GenerateUpdateableChangeActions]
    public UpdatePropertyValue_Change(Guid nodeId, string property, object? value)
    {
        _nodeId = nodeId;
        _propertyName = property;
        _value = value;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (target.TryFindNode<Node>(_nodeId, out var node))
        {
            var property = node.GetInputProperty(_propertyName);
            if (property == null) return false;

            previousValue = GetValue(property);
            if (previousValue is ShaderExpressionVariable expr)
            {
                previousValue = expr.GetConstant();
            }

            return true;
        }

        return false;
    }

    [UpdateChangeMethod]
    public void UpdateValue(object? value)
    {
        _value = value;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        var node = target.NodeGraph.Nodes.First(x => x.Id == _nodeId);
        var property = node.GetInputProperty(_propertyName);

        int inputsHash = GraphUtils.CalculateInputsHash(node);
        int outputsHash = GraphUtils.CalculateOutputsHash(node);

        string errors = string.Empty;
        if (!property.Validator.Validate(_value, out errors))
        {
            if (string.IsNullOrEmpty(errors))
            {
                _value = property.Validator.GetClosestValidValue(_value);
            }

            _value = SetValue(property, _value);
        }
        else
        {
            _value = SetValue(property, _value);
        }

        if(property.Node is BlackboardVariableValueNode blackboardNode)
            blackboardNode.UpdateValuesFromBlackboard(target.NodeGraph.Blackboard);

        List<IChangeInfo> changes = new();
        changes.Add(new PropertyValueUpdated_ChangeInfo(_nodeId, _propertyName, _value) { Errors = errors });

        int newInputsHash = GraphUtils.CalculateInputsHash(node);
        int newOutputsHash = GraphUtils.CalculateOutputsHash(node);

        if (inputsHash != newInputsHash)
        {
            changes.Add(NodeInputsChanged_ChangeInfo.FromNode(node));
        }

        if (outputsHash != newOutputsHash)
        {
            changes.Add(NodeOutputsChanged_ChangeInfo.FromNode(node));
        }

        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        var node = target.NodeGraph.Nodes.First(x => x.Id == _nodeId);
        var property = node.GetInputProperty(_propertyName);

        int inputsHash = GraphUtils.CalculateInputsHash(node);
        int outputsHash = GraphUtils.CalculateOutputsHash(node);

        string errors = string.Empty;
        if (!property.Validator.Validate(_value, out errors))
        {
            if (string.IsNullOrEmpty(errors))
            {
                _value = property.Validator.GetClosestValidValue(_value);
                if (_value == previousValue)
                {
                    ignoreInUndo = true;
                    return new None();
                }
            }

            _value = SetValue(property, _value);
            ignoreInUndo = false;
        }
        else
        {
            _value = SetValue(property, _value);
            ignoreInUndo = false;
        }

        List<IChangeInfo> changes = new();
        changes.Add(new PropertyValueUpdated_ChangeInfo(_nodeId, _propertyName, _value) { Errors = errors });

        int newInputsHash = GraphUtils.CalculateInputsHash(node);
        int newOutputsHash = GraphUtils.CalculateOutputsHash(node);

        if (inputsHash != newInputsHash)
        {
            changes.Add(NodeInputsChanged_ChangeInfo.FromNode(node));
        }

        if (outputsHash != newOutputsHash)
        {
            changes.Add(NodeOutputsChanged_ChangeInfo.FromNode(node));
        }

        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var node = target.NodeGraph.Nodes.First(x => x.Id == _nodeId);
        var property = node.GetInputProperty(_propertyName);

        int inputsHash = GraphUtils.CalculateInputsHash(node);
        int outputsHash = GraphUtils.CalculateOutputsHash(node);

        SetValue(property, previousValue);

        List<IChangeInfo> changes = new();

        changes.Add(new PropertyValueUpdated_ChangeInfo(_nodeId, _propertyName, previousValue));

        int newInputsHash = GraphUtils.CalculateInputsHash(node);
        int newOutputsHash = GraphUtils.CalculateOutputsHash(node);

        if (inputsHash != newInputsHash)
        {
            changes.Add(NodeInputsChanged_ChangeInfo.FromNode(node));
        }

        if (outputsHash != newOutputsHash)
        {
            changes.Add(NodeOutputsChanged_ChangeInfo.FromNode(node));
        }

        return changes;
    }

    private static object SetValue(InputProperty property, object? value)
    {
        if (property is IFuncInputProperty fieldInput)
        {
            fieldInput.SetFuncConstantValue(value);
        }
        else
        {
            if (value is int && property.ValueType.IsEnum)
            {
                value = Enum.ToObject(property.ValueType, value);
            }

            property.NonOverridenValue = value;
        }

        return value;
    }


    private static object? GetValue(InputProperty property)
    {
        if (property is IFuncInputProperty fieldInput)
        {
            return fieldInput.GetFuncConstantValue();
        }

        return property.NonOverridenValue;
    }

    public override bool IsMergeableWith(Change other)
    {
        return other is UpdatePropertyValue_Change change && change._nodeId == _nodeId &&
               change._propertyName == _propertyName && _value == change._value;
    }

    public override void Dispose()
    {
        if(previousValue is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
