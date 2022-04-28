using PixiEditor.ChangeableDocument.Actions.Properties;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditorPrototype.Models;

namespace PixiEditorPrototype.ViewModels;

internal class LayerViewModel : StructureMemberViewModel
{
    public bool LockTransparency
    {
        get => ((IReadOnlyLayer)member).LockTransparency;
        set => Helpers.ActionAccumulator.AddFinishedActions(new SetLayerLockTransparency_Action(member.GuidValue, value));
    }
    public LayerViewModel(DocumentViewModel doc, DocumentHelpers helpers, IReadOnlyLayer layer) : base(doc, helpers, layer)
    {
    }
}
