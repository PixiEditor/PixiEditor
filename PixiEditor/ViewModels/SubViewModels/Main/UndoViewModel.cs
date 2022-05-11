using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Undo;
using System.IO;
using System.Windows.Input;
using PixiEditor.Models.Services;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    [Command.Group("PixiEditor.Undo", "Undo")]
    public class UndoViewModel : SubViewModel<ViewModelMain>
    {
        private readonly DocumentProvider _doc;
        
        public event EventHandler UndoRedoCalled;

        public UndoViewModel(ViewModelMain owner, DocumentProvider provider)
            : base(owner)
        {
            var result = Directory.CreateDirectory(StorageBasedChange.DefaultUndoChangeLocation);

            _doc = provider;
        }

        /// <summary>
        ///     Redo last action.
        /// </summary>
        [Command.Basic("PixiEditor.Undo.Redo", "Redo", "Redo next step", CanExecute = "PixiEditor.Undo.CanRedo", Key = Key.Y, Modifiers = ModifierKeys.Control,
                       Icon = "E7A6", IconEvaluator = "PixiEditor.FontIcon")]
        public void Redo()
        {
            UndoRedoCalled?.Invoke(this, EventArgs.Empty);

            //sometimes CanRedo gets changed after UndoRedoCalled invoke, so check again (normally this is checked by the relaycommand)
            if (CanRedo())
            {
                Owner.BitmapManager.ActiveDocument.UndoManager.Redo();
                Owner.BitmapManager.ActiveDocument.ChangesSaved = false;
            }
        }

        /// <summary>
        ///     Undo last action.
        /// </summary>
        [Command.Basic("PixiEditor.Undo.Undo", "Undo", "Undo previous step", CanExecute = "PixiEditor.Undo.CanUndo", Key = Key.Z, Modifiers = ModifierKeys.Control,
         Icon = "E7A7", IconEvaluator = "PixiEditor.FontIcon")]
        public void Undo()
        {
            UndoRedoCalled?.Invoke(this, EventArgs.Empty);

            //sometimes CanUndo gets changed after UndoRedoCalled invoke, so check again (normally this is checked by the relaycommand)
            if (CanUndo())
            {
                _doc.GetDocument().UndoManager.Undo();
                _doc.GetDocument().ChangesSaved = false;
            }
        }

        /// <summary>
        /// Removes all files from %tmp%/PixiEditor/UndoStack/.
        /// </summary>
        public void ClearUndoTempDirectory()
        {
            DirectoryInfo dirInfo = new DirectoryInfo(StorageBasedChange.DefaultUndoChangeLocation);
            foreach (FileInfo file in dirInfo.GetFiles())
            {
                file.Delete();
            }
        }

        /// <summary>
        ///     Returns true if undo can be done.
        /// </summary>
        /// <param name="property">CommandParameter.</param>
        /// <returns>True if can undo.</returns>
        [Evaluator.CanExecute("PixiEditor.Undo.CanUndo", requires: "PixiEditor.HasDocument")]
        public bool CanUndo()
        {
            return _doc.GetDocument().UndoManager.CanUndo;
        }

        /// <summary>
        ///     Returns true if redo can be done.
        /// </summary>
        /// <param name="property">CommandProperty.</param>
        /// <returns>True if can redo.</returns>
        [Evaluator.CanExecute("PixiEditor.Undo.CanRedo", requires: "PixiEditor.HasDocument")]
        public bool CanRedo()
        {
            return _doc.GetDocument().UndoManager.CanRedo;
        }
    }
}
