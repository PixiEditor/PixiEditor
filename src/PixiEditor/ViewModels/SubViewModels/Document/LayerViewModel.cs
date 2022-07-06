using PixiEditor.ChangeableDocument.Actions.Generated;
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
        set => Helpers.ActionAccumulator.AddFinishedActions(new LayerLockTransparency_Action(GuidValue, value));
    }
    public LayerViewModel(DocumentViewModel doc, DocumentHelpers helpers, Guid guidValue) : base(doc, helpers, guidValue)
    {
    }
}
