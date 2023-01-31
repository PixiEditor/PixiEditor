using System.Windows.Input;
using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Commands.Attributes.Evaluators;

namespace PixiEditor.ViewModels.SubViewModels.Main;

[Command.Group("PixiEditor.Undo", "Undo")]
internal class UndoViewModel : SubViewModel<ViewModelMain>
{
    public UndoViewModel(ViewModelMain owner)
        : base(owner)
    {
    }

    /// <summary>
    ///     Redo last action.
    /// </summary>
    [Command.Basic("PixiEditor.Undo.Redo", "Redo", "Redo next step", CanExecute = "PixiEditor.Undo.CanRedo", Key = Key.Y, Modifiers = ModifierKeys.Control,
        IconPath = "E7A6", IconEvaluator = "PixiEditor.FontIcon")]
    public void Redo()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null || (!doc.UpdateableChangeActive && !doc.HasSavedRedo))
            return;
        doc.Operations.Redo();
    }

    /// <summary>
    ///     Undo last action.
    /// </summary>
    [Command.Basic("PixiEditor.Undo.Undo", "Undo", "Undo previous step", CanExecute = "PixiEditor.Undo.CanUndo", Key = Key.Z, Modifiers = ModifierKeys.Control,
        IconPath = "E7A7", IconEvaluator = "PixiEditor.FontIcon")]
    public void Undo()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null || (!doc.UpdateableChangeActive && !doc.HasSavedUndo))
            return;
        doc.Operations.Undo();
    }

    /// <summary>
    ///     Returns true if undo can be done.
    /// </summary>
    /// <param name="property">CommandParameter.</param>
    /// <returns>True if can undo.</returns>
    [Evaluator.CanExecute("PixiEditor.Undo.CanUndo")]
    public bool CanUndo()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return false;
        return doc.UpdateableChangeActive || doc.HasSavedUndo;
    }

    /// <summary>
    ///     Returns true if redo can be done.
    /// </summary>
    /// <param name="property">CommandProperty.</param>
    /// <returns>True if can redo.</returns>
    [Evaluator.CanExecute("PixiEditor.Undo.CanRedo")]
    public bool CanRedo()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return false;
        return doc.UpdateableChangeActive || doc.HasSavedRedo;
    }
}
