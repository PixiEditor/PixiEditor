using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

internal class UpdatePropertyValue_Change : Change
{
    private readonly Guid _nodeId;
    private readonly string _propertyName;
    private readonly object? _value;
    private object? previousValue;
    
    [GenerateMakeChangeAction]
    public UpdatePropertyValue_Change(Guid nodeId, string property, object? value)
    {
        _nodeId = nodeId;
        _propertyName = property;
        _value = value;
    }
    
    public override bool InitializeAndValidate(Document target) => true;

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        var node = target.NodeGraph.Nodes.First(x => x.Id == _nodeId);
        var property = node.GetInputProperty(_propertyName);

        previousValue = GetValue(property);
        SetValue(property, _value);

        ignoreInUndo = false;
        
        return new PropertyValueUpdated_ChangeInfo(_nodeId, _propertyName, _value);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var node = target.NodeGraph.Nodes.First(x => x.Id == _nodeId);
        var property = node.GetInputProperty(_propertyName);
        SetValue(property, previousValue);
        
        return new PropertyValueUpdated_ChangeInfo(_nodeId, _propertyName, previousValue);
    }

    private static void SetValue(InputProperty property, object? value)
    {
        if (property is IFuncInputProperty fieldInput)
        {
            fieldInput.SetFuncConstantValue(value);
        }
        else
        {
            property.NonOverridenValue = value;
        }
    }
    

    private static object? GetValue(InputProperty property)
    {
        if (property is IFuncInputProperty fieldInput)
        {
            return fieldInput.GetFuncConstantValue();
        }

        return property.NonOverridenValue;
    }
}
