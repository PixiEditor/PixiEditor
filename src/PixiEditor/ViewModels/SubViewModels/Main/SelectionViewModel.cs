using System.Windows.Input;
using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Tools.Tools;

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

    }

    [Command.Basic("PixiEditor.Selection.Clear", "Clear selection", "Clear selection", CanExecute = "PixiEditor.Selection.IsNotEmpty", Key = Key.D, Modifiers = ModifierKeys.Control)]
    public void Deselect()
    {

    }

    [Evaluator.CanExecute("PixiEditor.Selection.IsNotEmpty")]
    public bool SelectionIsNotEmpty()
    {
        return false;
    }
}
