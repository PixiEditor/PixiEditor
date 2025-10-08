using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph.Blackboard;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph.Blackboard;

internal class AddBlackboardVariable_Change : Change
{
    private string varName;
    private Type type;
    private double min;
    private double max;
    private string? unit;

    [GenerateMakeChangeAction]
    public AddBlackboardVariable_Change(Type type, double min = double.NaN, double max = double.NaN, string? unit = null)
    {
        this.type = type;
        this.min = min;
        this.max = max;
        this.unit = unit;
    }

    public override bool InitializeAndValidate(Document target)
    {
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        int variablesOfType = target.NodeGraph.Blackboard.Variables.Values.Count(v => v.Type == type);
        string name = $"{type.Name.ToLower()}{variablesOfType + 1}";
        if(NameExists(target, name))
        {
            int i = 1;
            while(NameExists(target, $"{name}_{i}"))
            {
                i++;
            }
            name = $"{name}_{i}";
        }

        varName = name;
        object value = Activator.CreateInstance(type)!;
        target.NodeGraph.Blackboard.SetVariable(name, type, value, unit, double.IsNaN(min) ? null : min, double.IsNaN(max) ? null : max);
        ignoreInUndo = false;

        return new BlackboardVariable_ChangeInfo(name, type, value, min, max, unit);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.NodeGraph.Blackboard.RemoveVariable(varName);
        return new BlackboardVariable_ChangeInfo(varName, type, null, min, max, unit);
    }

    private bool NameExists(Document target, string name)
    {
        return target.NodeGraph.Blackboard.Variables.ContainsKey(name);
    }
}
