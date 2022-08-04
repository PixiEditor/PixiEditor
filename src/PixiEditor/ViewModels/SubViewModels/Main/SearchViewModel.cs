using System.Windows.Input;
using PixiEditor.Models.Commands.Attributes.Commands;

namespace PixiEditor.ViewModels.SubViewModels.Main;

[Command.Group("PixiEditor.Search", "Search")]
internal class SearchViewModel : SubViewModel<ViewModelMain>
{
    private bool searchWindowOpen;
    private string searchTerm;

    public bool SearchWindowOpen
    {
        get => searchWindowOpen;
        set => SetProperty(ref searchWindowOpen, value);
    }

    public string SearchTerm
    {
        get => searchTerm;
        set => SetProperty(ref searchTerm, value);
    }

    public SearchViewModel(ViewModelMain owner) : base(owner)
    { }

    [Evaluator.CanExecute("PixiEditor.Search.CanOpenSearchWindow")]
    public bool CanToggleSeachWindow() => !ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Busy ?? true;

    [Command.Basic("PixiEditor.Search.Toggle", "", "Command Search", "Open the command search window", Key = Key.K, Modifiers = ModifierKeys.Control, CanExecute = "PixiEditor.Search.CanOpenSearchWindow")]
    public void ToggleSearchWindow(string searchTerm)
    {
        SearchWindowOpen = !SearchWindowOpen;
        if (SearchWindowOpen)
        {
            SearchTerm = searchTerm;
        }
    }
}
