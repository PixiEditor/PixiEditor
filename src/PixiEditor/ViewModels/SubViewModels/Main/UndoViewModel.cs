using System.Windows.Input;
using PixiEditor.Models.Commands.Attributes;

namespace PixiEditor.ViewModels.SubViewModels.Main;

[Command.Group("PixiEditor.Undo", "Undo")]
public class UndoViewModel : SubViewModel<ViewModelMain>
{
    public event EventHandler UndoRedoCalled;

    public UndoViewModel(ViewModelMain owner)
        : base(owner)
    {
        //var result = Directory.CreateDirectory(StorageBasedChange.DefaultUndoChangeLocation);

        //ClearUndoTempDirectory();
    }

    /// <summary>
    ///     Redo last action.
    /// </summary>
    [Command.Basic("PixiEditor.Undo.Redo", "Redo", "Redo next step", CanExecute = "PixiEditor.Undo.CanRedo", Key = Key.Y, Modifiers = ModifierKeys.Control,
        IconPath = "E7A6", IconEvaluator = "PixiEditor.FontIcon")]
    public void Redo()
    {
        UndoRedoCalled?.Invoke(this, EventArgs.Empty);

        //sometimes CanRedo gets changed after UndoRedoCalled invoke, so check again (normally this is checked by the relaycommand)
        if (CanRedo())
        {
            //Owner.BitmapManager.ActiveDocument.UndoManager.Redo();
            //Owner.BitmapManager.ActiveDocument.ChangesSaved = false;
        }
    }

    /// <summary>
    ///     Undo last action.
    /// </summary>
    [Command.Basic("PixiEditor.Undo.Undo", "Undo", "Undo previous step", CanExecute = "PixiEditor.Undo.CanUndo", Key = Key.Z, Modifiers = ModifierKeys.Control,
        IconPath = "E7A7", IconEvaluator = "PixiEditor.FontIcon")]
    public void Undo()
    {
        UndoRedoCalled?.Invoke(this, EventArgs.Empty);

        //sometimes CanUndo gets changed after UndoRedoCalled invoke, so check again (normally this is checked by the relaycommand)
        if (CanUndo())
        {
            //Owner.BitmapManager.ActiveDocument.UndoManager.Undo();
            //Owner.BitmapManager.ActiveDocument.ChangesSaved = false;
        }
    }

    /// <summary>
    /// Removes all files from %tmp%/PixiEditor/UndoStack/.
    /// </summary>
    public void ClearUndoTempDirectory()
    {
        /*DirectoryInfo dirInfo = new DirectoryInfo(StorageBasedChange.DefaultUndoChangeLocation);
        foreach (FileInfo file in dirInfo.GetFiles())
        {
            file.Delete();
        }*/
    }

    /// <summary>
    ///     Returns true if undo can be done.
    /// </summary>
    /// <param name="property">CommandParameter.</param>
    /// <returns>True if can undo.</returns>
    [Evaluator.CanExecute("PixiEditor.Undo.CanUndo")]
    public bool CanUndo()
    {
        return false;
    }

    /// <summary>
    ///     Returns true if redo can be done.
    /// </summary>
    /// <param name="property">CommandProperty.</param>
    /// <returns>True if can redo.</returns>
    [Evaluator.CanExecute("PixiEditor.Undo.CanRedo")]
    public bool CanRedo()
    {
        return false;
    }
}
