using PixiEditor.Helpers;
using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.Tools;
using System.Windows.Input;
using PixiEditor.Models.Services;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    [Command.Group("PixiEditor.Selection", "Selection")]
    public class SelectionViewModel : SubViewModel<ViewModelMain>
    {
        private readonly SelectTool selectTool;
        private readonly DocumentProvider _doc;

        public SelectionViewModel(ViewModelMain owner, DocumentProvider provider)
            : base(owner)
        {
            selectTool = new SelectTool(Owner.BitmapManager);
        }

        [Command.Basic("PixiEditor.Selection.SelectAll", "Select all", "Select everything", CanExecute = "PixiEditor.HasDocument", Key = Key.A, Modifiers = ModifierKeys.Control)]
        public void SelectAll()
        {
            var oldSelection = new List<Coordinates>(Owner.BitmapManager.ActiveDocument.ActiveSelection.SelectedPoints);

            _doc.GetDocument().ActiveSelection.SetSelection(selectTool.GetAllSelection(), SelectionType.New);
            SelectionHelpers.AddSelectionUndoStep(_doc.GetDocument(), oldSelection, SelectionType.New);
        }

        [Command.Basic("PixiEditor.Selection.Clear", "Clear selection", "Clear selection", CanExecute = "PixiEditor.Selection.IsNotEmpty", Key = Key.D, Modifiers = ModifierKeys.Control)]
        public void Deselect()
        {
            var oldSelection = new List<Coordinates>(_doc.GetDocument().ActiveSelection.SelectedPoints);

            _doc.GetDocument().ActiveSelection?.Clear();

            SelectionHelpers.AddSelectionUndoStep(_doc.GetDocument(), oldSelection, SelectionType.New);
        }

        [Evaluator.CanExecute("PixiEditor.Selection.IsNotEmpty")]
        public bool SelectionIsNotEmpty()
        {
            var selectedPoints = _doc.GetDocument().ActiveSelection.SelectedPoints;
            return selectedPoints != null && selectedPoints.Count > 0;
        }
    }
}