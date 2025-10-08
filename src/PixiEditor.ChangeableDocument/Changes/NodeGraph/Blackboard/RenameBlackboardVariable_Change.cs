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
        ignoreInUndo = false;

        return new RenameBlackboardVariable_ChangeInfo(oldName, newName);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        throw new NotImplementedException();
    }
}
