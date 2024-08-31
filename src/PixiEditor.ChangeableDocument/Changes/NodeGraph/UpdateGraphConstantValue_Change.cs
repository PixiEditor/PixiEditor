using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

internal class UpdateGraphConstantValue_Change : Change
{
    private object oldValue;
    
    public Guid Id { get; }
    
    public object Value { get; }
    
    [GenerateMakeChangeAction]
    public UpdateGraphConstantValue_Change(Guid id, object value)
    {
        Id = id;
        Value = value;
    }

    public override bool InitializeAndValidate(Document target) => true;

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        if (firstApply)
            target.NodeGraph.UpdateConstantValue(Id, Value, out oldValue);

        ignoreInUndo = false;
        return new UpdateConstantValue_ChangeInfo(Id, Value);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        return new UpdateConstantValue_ChangeInfo(Id, oldValue);
    }
}
