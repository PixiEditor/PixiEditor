using Avalonia.Input;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Commands.Attributes.Evaluators;
using PixiEditor.Models.DocumentModels.UpdateableChangeExecutors.Features;
using PixiEditor.Models.Input;
using PixiEditor.UI.Common.Fonts;

namespace PixiEditor.ViewModels.SubViewModels;

[Command.Group("PixiEditor.Undo", "UNDO")]
internal class UndoViewModel : SubViewModel<ViewModelMain>
{
    public UndoViewModel(ViewModelMain owner)
        : base(owner)
    {
    }

    /// <summary>
    ///     Redo last action.
    /// </summary>
    [Command.Basic("PixiEditor.Undo.Redo", "REDO", "REDO_DESCRIPTIVE", CanExecute = "PixiEditor.Undo.CanRedo", 
        Key = Key.Y, Modifiers = KeyModifiers.Control,
        Icon = PixiPerfectIcons.Redo, MenuItemPath = "EDIT/REDO", MenuItemOrder = 1, AnalyticsTrack = true)]
    [CustomOsShortcut("PixiEditor.Undo.Redo", "MacOS", Key.Z, KeyModifiers.Meta | KeyModifiers.Shift)]
    public void Redo()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null || (!doc.IsChangeFeatureActive<IMidChangeUndoableExecutor>() && !doc.HasSavedRedo))
            return;
        
        doc.Operations.InvokeCustomAction(
            () =>
        {
            Owner.ToolsSubViewModel.OnPreRedoInlet();
        }, false);
        doc.Operations.Redo();
        doc.Operations.InvokeCustomAction(
            () =>
        {
            Owner.ToolsSubViewModel.OnPostRedoInlet();
        }, false);
    }

    /// <summary>
    ///     Undo last action.
    /// </summary>
    [Command.Basic("PixiEditor.Undo.Undo", "UNDO", "UNDO_DESCRIPTIVE", 
        CanExecute = "PixiEditor.Undo.CanUndo", 
        Key = Key.Z, Modifiers = KeyModifiers.Control,
        Icon = PixiPerfectIcons.Undo, MenuItemPath = "EDIT/UNDO", MenuItemOrder = 0, AnalyticsTrack = true)]
    public void Undo()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null || (!doc.IsChangeFeatureActive<IMidChangeUndoableExecutor>() && !doc.HasSavedUndo))
            return;
        doc.Operations.InvokeCustomAction(
            () =>
        {
            Owner.ToolsSubViewModel.OnPreUndoInlet();
        }, false);
        doc.Operations.Undo();
        doc.Operations.InvokeCustomAction(
            () =>
        {
            Owner.ToolsSubViewModel.OnPostUndoInlet();
        }, false);
    }

    /// <summary>
    ///     Returns true if undo can be done.
    /// </summary>
    /// <param name="property">CommandParameter.</param>
    /// <returns>True if can undo.</returns>
    [Evaluator.CanExecute("PixiEditor.Undo.CanUndo", 
        nameof(ViewModelMain.DocumentManagerSubViewModel), 
        nameof(ViewModelMain.DocumentManagerSubViewModel.ActiveDocument), 
        nameof(ViewModelMain.DocumentManagerSubViewModel.ActiveDocument.HasSavedUndo))]
    public bool CanUndo()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return false;
        
        IMidChangeUndoableExecutor executor = doc.TryGetExecutorFeature<IMidChangeUndoableExecutor>();
        
        return executor is { CanUndo: true } || doc.HasSavedUndo;
    }

    /// <summary>
    ///     Returns true if redo can be done.
    /// </summary>
    /// <param name="property">CommandProperty.</param>
    /// <returns>True if can redo.</returns>
    [Evaluator.CanExecute("PixiEditor.Undo.CanRedo", 
        nameof(ViewModelMain.DocumentManagerSubViewModel), 
        nameof(ViewModelMain.DocumentManagerSubViewModel.ActiveDocument), 
        nameof(ViewModelMain.DocumentManagerSubViewModel.ActiveDocument.HasSavedRedo))]
    public bool CanRedo()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return false;
        
        IMidChangeUndoableExecutor executor = doc.TryGetExecutorFeature<IMidChangeUndoableExecutor>();
        
        return executor is { CanRedo: true } || doc.HasSavedRedo;
    }
}
