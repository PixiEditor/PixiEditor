using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;

namespace PixiEditor.ViewModels.Document;
#nullable enable
internal class LayerViewModel : StructureMemberViewModel, ILayerHandler
{
    public LayerViewModel()
    {
        
    }
    
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
            if (!Document.UpdateableChangeActive)
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

    public LayerViewModel(DocumentViewModel doc, DocumentInternalParts internals, Guid id) : base(doc, internals, id)
    {
    }
}
