using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

namespace PixiEditor.ChangeableDocument.Changes.Structure;

internal class SetLowDpiRendering_Change : Change
{
    public readonly Guid memberId;
    public bool value;
    
    private bool originalValue;
    
    [GenerateMakeChangeAction]
    public SetLowDpiRendering_Change(Guid memberId, bool value)
    {
        this.memberId = memberId;
        this.value = value;
    }
    
    public override bool InitializeAndValidate(Document target)
    {
        return target.TryFindNode(memberId, out RenderNode node);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        RenderNode node = target.FindNodeOrThrow<RenderNode>(memberId);
        
        bool toSet = !value;
        
        originalValue = node.AllowHighDpiRendering;
        node.AllowHighDpiRendering = toSet;
        
        ignoreInUndo = originalValue == toSet;

        return new None();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        RenderNode node = target.FindNodeOrThrow<RenderNode>(memberId);
        
        node.AllowHighDpiRendering = originalValue;

        return new None();
    }
}
