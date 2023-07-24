using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.Models.Containers;
using PixiEditor.Models.DocumentModels;

namespace PixiEditor.ViewModels.SubViewModels.Document;
#nullable enable
internal class LayerViewModel : StructureMemberViewModel, ILayerHandler
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
            if (!Document.UpdateableChangeActive)
                Internals.ActionAccumulator.AddFinishedActions(new LayerLockTransparency_Action(GuidValue, value));
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

    public LayerViewModel(DocumentViewModel doc, DocumentInternalParts internals, Guid guidValue) : base(doc, internals, guidValue)
    {
    }
}
