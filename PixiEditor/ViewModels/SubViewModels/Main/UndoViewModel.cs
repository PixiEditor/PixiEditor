using System;
using System.IO;
using System.Linq;
using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Tools;
using PixiEditor.Models.Undo;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class UndoViewModel : SubViewModel<ViewModelMain>
    {
        public RelayCommand UndoCommand { get; set; }

        public RelayCommand RedoCommand { get; set; }

        private LayerChange[] undoChanges;

        public LayerChange[] UndoChanges // This acts like UndoManager process, but it was implemented before process system, so it can be transformed into it
        {
            get => undoChanges;
            set
            {
                undoChanges = value;
                for (int i = 0; i < value.Length; i++)
                {
                    Owner.BitmapManager.ActiveDocument.Layers.First(x => x.LayerGuid == value[i].LayerGuid).SetPixels(value[i].PixelChanges);
                }
            }
        }

        public UndoViewModel(ViewModelMain owner)
            : base(owner)
        {
            UndoCommand = new RelayCommand(Undo, CanUndo);
            RedoCommand = new RelayCommand(Redo, CanRedo);
            if (!Directory.Exists(StorageBasedChange.DefaultUndoChangeLocation))
            {
                Directory.CreateDirectory(StorageBasedChange.DefaultUndoChangeLocation);
            }

            ClearUndoTempDirectory();
        }

        public void TriggerNewUndoChange(Tool toolUsed)
        {
            if (BitmapManager.IsOperationTool(toolUsed)
                && ((BitmapOperationTool)toolUsed).UseDefaultUndoMethod)
            {
                Tuple<LayerChange, LayerChange>[] changes = Owner.ChangesController.PopChanges();
                if (changes != null && changes.Length > 0)
                {
                    LayerChange[] newValues = changes.Select(x => x.Item1).ToArray();
                    LayerChange[] oldValues = changes.Select(x => x.Item2).ToArray();
                    Owner.BitmapManager.ActiveDocument.UndoManager.AddUndoChange(
                        new Change("UndoChanges", oldValues, newValues, root: this));
                    toolUsed.AfterAddedUndo(Owner.BitmapManager.ActiveDocument.UndoManager);
                }
            }
        }

        /// <summary>
        ///     Redo last action.
        /// </summary>
        /// <param name="parameter">CommandProperty.</param>
        public void Redo(object parameter)
        {
            Owner.BitmapManager.ActiveDocument.UndoManager.Redo();
        }

        /// <summary>
        ///     Undo last action.
        /// </summary>
        /// <param name="parameter">CommandParameter.</param>
        public void Undo(object parameter)
        {
            Owner.SelectionSubViewModel.Deselect(false);
            Owner.BitmapManager.ActiveDocument.UndoManager.Undo();
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
        private bool CanUndo(object property)
        {
            return Owner.BitmapManager.ActiveDocument?.UndoManager.CanUndo ?? false;
        }

        /// <summary>
        ///     Returns true if redo can be done.
        /// </summary>
        /// <param name="property">CommandProperty.</param>
        /// <returns>True if can redo.</returns>
        private bool CanRedo(object property)
        {
            return Owner.BitmapManager.ActiveDocument?.UndoManager.CanRedo ?? false;
        }
    }
}