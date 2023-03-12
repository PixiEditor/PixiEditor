using System.Windows.Input;
using PixiEditor.Models.Commands.Attributes.Commands;

namespace PixiEditor.ViewModels.SubViewModels.Main;

[Command.Group("PixiEditor.Search", "SEARCH")]
internal class SearchViewModel : SubViewModel<ViewModelMain>
{
    private bool searchWindowOpen;
    private bool selectAll;
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

    public bool SelectAll
    {
        get => selectAll;
        set => SetProperty(ref selectAll, value);
    }

    public SearchViewModel(ViewModelMain owner) : base(owner)
    { }

    [Evaluator.CanExecute("PixiEditor.Search.CanOpenSearchWindow")]
    public bool CanToggleSeachWindow() => !ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Busy ?? true;

    [Command.Basic("PixiEditor.Search.Toggle", "", "Command Search", "Open the command search window", Key = Key.K, Modifiers = ModifierKeys.Control, CanExecute = "PixiEditor.Search.CanOpenSearchWindow")]
    public void ToggleSearchWindow(string searchTerm)
    {
        SelectAll = true;
        SearchWindowOpen = !SearchWindowOpen;
        if (SearchWindowOpen)
        {
            SearchTerm = searchTerm;
        }
    }

    public void OpenSearchWindow(string searchTerm, bool selectAll = true)
    {
        SelectAll = selectAll;
        SearchWindowOpen = true;
        SearchTerm = searchTerm;
    }
}
