using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

internal class CreateGraphConstant_Change : Change
{
    private Guid Id { get; }
    
    private Type Type { get; }

    [GenerateMakeChangeAction]
    public CreateGraphConstant_Change(Guid id, Type type)
    {
        Id = id;
        Type = type;
    }

    public override bool InitializeAndValidate(Document target) => true;

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        target.NodeGraph.AddConstant(new GraphConstant(Id, Type));

        ignoreInUndo = false;
        return new CreateConstant_ChangeInfo(Id, Type);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.NodeGraph.RemoveConstant(Id);
        
        return new DeleteConstant_ChangeInfo(Id);
    }
}
