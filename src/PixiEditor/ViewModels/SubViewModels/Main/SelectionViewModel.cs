using PixiEditor.Helpers;
using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.Tools;
using System.Windows.Input;

namespace PixiEditor.ViewModels.SubViewModels.Main;

[Command.Group("PixiEditor.Selection", "Selection")]
public class SelectionViewModel : SubViewModel<ViewModelMain>
{
    private readonly SelectTool selectTool;

    public SelectionViewModel(ViewModelMain owner)
        : base(owner)
    {
        selectTool = new SelectTool(Owner.BitmapManager);
    }

    [Command.Basic("PixiEditor.Selection.SelectAll", "Select all", "Select everything", CanExecute = "PixiEditor.HasDocument", Key = Key.A, Modifiers = ModifierKeys.Control)]
    public void SelectAll()
    {
        var oldSelection = new List<Coordinates>(Owner.BitmapManager.ActiveDocument.ActiveSelection.SelectedPoints);

        Owner.BitmapManager.ActiveDocument.ActiveSelection.SetSelection(selectTool.GetAllSelection(), SelectionType.New);
        SelectionHelpers.AddSelectionUndoStep(Owner.BitmapManager.ActiveDocument, oldSelection, SelectionType.New);
    }

    [Command.Basic("PixiEditor.Selection.Clear", "Clear selection", "Clear selection", CanExecute = "PixiEditor.Selection.IsNotEmpty", Key = Key.D, Modifiers = ModifierKeys.Control)]
    public void Deselect()
    {
        var oldSelection = new List<Coordinates>(Owner.BitmapManager.ActiveDocument.ActiveSelection.SelectedPoints);

        Owner.BitmapManager.ActiveDocument.ActiveSelection?.Clear();

        SelectionHelpers.AddSelectionUndoStep(Owner.BitmapManager.ActiveDocument, oldSelection, SelectionType.New);
    }

    [Evaluator.CanExecute("PixiEditor.Selection.IsNotEmpty")]
    public bool SelectionIsNotEmpty()
    {
        var selectedPoints = Owner.BitmapManager.ActiveDocument?.ActiveSelection.SelectedPoints;
        return selectedPoints != null && selectedPoints.Count > 0;
    }
}