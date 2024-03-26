using Avalonia.Input;
using PixiEditor.AvaloniaUI.Models.Commands.Attributes.Commands;
using PixiEditor.AvaloniaUI.Models.Commands.Attributes.Evaluators;
using PixiEditor.AvaloniaUI.Models.Handlers;

namespace PixiEditor.AvaloniaUI.ViewModels.SubViewModels;

[Command.Group("PixiEditor.Search", "SEARCH")]
internal class SearchViewModel : SubViewModel<ViewModelMain>, ISearchHandler
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
    public bool CanToggleSearchWindow() => !ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Busy ?? true;

    [Command.Basic("PixiEditor.Search.Toggle", "", "COMMAND_SEARCH", "OPEN_COMMAND_SEARCH", Key = Key.K, Modifiers = KeyModifiers.Control, CanExecute = "PixiEditor.Search.CanOpenSearchWindow")]
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
