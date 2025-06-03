using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Models.Handlers;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.Nodes;
using PixiEditor.ViewModels.Tools.Tools;

namespace PixiEditor.ViewModels.Document.Nodes;

[NodeViewModel("IMAGE_LAYER_NODE", "STRUCTURE", PixiPerfectIcons.LayersDouble)]
internal class ImageLayerNodeViewModel : StructureMemberViewModel<ImageLayerNode>, ITransparencyLockableMember, IRasterLayerHandler
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

    public Type? QuickEditTool { get; } = typeof(PenToolViewModel);
}
