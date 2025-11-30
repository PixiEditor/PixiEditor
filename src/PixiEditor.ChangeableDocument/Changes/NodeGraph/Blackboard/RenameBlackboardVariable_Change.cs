using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph.Blackboard;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph.Blackboard;

internal class RenameBlackboardVariable_Change : Change
{
    public string oldName;
    public string newName;

    [GenerateMakeChangeAction]
    public RenameBlackboardVariable_Change(string oldName, string newName)
    {
        this.oldName = oldName;
        this.newName = newName;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (string.IsNullOrWhiteSpace(oldName) || string.IsNullOrWhiteSpace(newName))
            return false;
        if (!target.NodeGraph.Blackboard.Variables.ContainsKey(oldName))
            return false;
        if (target.NodeGraph.Blackboard.Variables.ContainsKey(newName))
            return false;
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        target.NodeGraph.Blackboard.RenameVariable(oldName, newName);
        var changes = UpdateReferences(target.NodeGraph, oldName, newName);
        ignoreInUndo = false;

        changes.Insert(0, new RenameBlackboardVariable_ChangeInfo(oldName, newName));

        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.NodeGraph.Blackboard.RenameVariable(newName, oldName);
        var changes = UpdateReferences(target.NodeGraph, newName, oldName);
        changes.Insert(0, new RenameBlackboardVariable_ChangeInfo(newName, oldName));
        return changes;
    }

    private List<IChangeInfo> UpdateReferences(Changeables.Graph.NodeGraph nodeGraph, string previous, string next)
    {
        var changeInfos = new List<IChangeInfo>();
        foreach (var node in nodeGraph.Nodes)
        {
            if (node is BlackboardVariableValueNode blackboardNode)
            {
                if (blackboardNode.VariableName.NonOverridenValue == previous)
                {
                    blackboardNode.VariableName.NonOverridenValue = next;
                    changeInfos.Add(new PropertyValueUpdated_ChangeInfo(blackboardNode.Id, BlackboardVariableValueNode.NameProperty, next));
                }
            }
        }

        return changeInfos;
    }
}
