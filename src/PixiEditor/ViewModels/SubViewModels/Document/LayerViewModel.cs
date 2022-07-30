using PixiEditor.Models.DocumentModels;

namespace PixiEditor.ViewModels.SubViewModels.Document;
#nullable enable
internal class LayerViewModel : StructureMemberViewModel
{
    bool lockTransparency;
    public void SetLockTransparency(bool lockTransparency)
    {
        this.lockTransparency = lockTransparency;
        RaisePropertyChanged(nameof(LockTransparencyBindable));
    }
    public bool LockTransparencyBindable
    {
        get => lockTransparency;
        set
        {
            if (!Document.UpdateableChangeActive)
                Helpers.ActionAccumulator.AddFinishedActions(new LayerLockTransparency_Action(GuidValue, value));
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
            RaisePropertyChanged(nameof(ShouldDrawOnMask));
        }
    }

    public LayerViewModel(DocumentViewModel doc, DocumentHelpers helpers, Guid guidValue) : base(doc, helpers, guidValue)
    {
    }
}
