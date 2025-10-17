using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.Models.Handlers;
using PixiEditor.ViewModels.Nodes;
using PixiEditor.ViewModels.Tools.Tools;

namespace PixiEditor.ViewModels.Document.Nodes;

[NodeViewModel("NESTED_DOCUMENT", null, null)]
internal class NestedDocumentNodeViewModel :
    StructureMemberViewModel<ChangeableDocument.Changeables.Graph.Nodes.NestedDocumentNode>, ILayerHandler
{
    bool lockTransparency;

    public void SetLockTransparency(bool lockTransparency)
    {
        this.lockTransparency = lockTransparency;
        OnPropertyChanged(nameof(LockTransparencyBindable));
    }

    public bool LockTransparencyBindable
    {
        get => lockTransparency;
        set
        {
            if (!Document.BlockingUpdateableChangeActive)
                Internals.ActionAccumulator.AddFinishedActions(new LayerLockTransparency_Action(Id, value));
        }
    }

    private bool shouldDrawOnMask = false;

    public bool ShouldDrawOnMask
    {
        get => shouldDrawOnMask;
        set
        {
            if (value == shouldDrawOnMask)
                return;
            shouldDrawOnMask = value;
            OnPropertyChanged(nameof(ShouldDrawOnMask));
        }
    }

    public Type? QuickEditTool => typeof(MoveToolViewModel);
}
