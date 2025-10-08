using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph.Blackboard;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph.Blackboard;

internal class AddBlackboardVariable_Change : Change
{
    private string varName;
    public Type type;

    [GenerateMakeChangeAction]
    public AddBlackboardVariable_Change(Type type)
    {
        this.type = type;
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
        target.NodeGraph.Blackboard.SetVariable(name, type, value);
        ignoreInUndo = false;

        return new BlackboardVariable_ChangeInfo(name, type, value);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.NodeGraph.Blackboard.RemoveVariable(varName);
        return new BlackboardVariable_ChangeInfo(varName, type, null);
    }

    private bool NameExists(Document target, string name)
    {
        return target.NodeGraph.Blackboard.Variables.ContainsKey(name);
    }
}
