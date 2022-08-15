using System.Windows.Input;
using PixiEditor.Models.Commands.Attributes.Commands;

namespace PixiEditor.ViewModels.SubViewModels.Main;

[Command.Group("PixiEditor.Selection", "Selection")]
internal class SelectionViewModel : SubViewModel<ViewModelMain>
{
    public SelectionViewModel(ViewModelMain owner)
        : base(owner)
    {
    }

    [Command.Basic("PixiEditor.Selection.SelectAll", "Select all", "Select everything", CanExecute = "PixiEditor.HasDocument", Key = Key.A, Modifiers = ModifierKeys.Control)]
    public void SelectAll()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;
        doc.Operations.SelectAll();
    }

    [Command.Basic("PixiEditor.Selection.Clear", "Clear selection", "Clear selection", CanExecute = "PixiEditor.Selection.IsNotEmpty", Key = Key.D, Modifiers = ModifierKeys.Control)]
    public void ClearSelection()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;
        doc.Operations.ClearSelection();
    }

    [Evaluator.CanExecute("PixiEditor.Selection.IsNotEmpty")]
    public bool SelectionIsNotEmpty()
    {
        return !Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectionPathBindable?.IsEmpty ?? false;
    }

    [Command.Basic("PixiEditor.Selection.TransformArea", "Transform selected area", "Transform selected area", CanExecute = "PixiEditor.Selection.IsNotEmpty", Key = Key.T, Modifiers = ModifierKeys.Control)]
    public void TransformSelectedArea()
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.Operations.TransformSelectedArea(false);
    }
}
