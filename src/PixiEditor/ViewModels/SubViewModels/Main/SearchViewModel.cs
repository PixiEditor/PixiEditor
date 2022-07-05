using System.Windows.Input;
using PixiEditor.Models.Commands.Attributes;
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

    [Command.Basic("PixiEditor.Search.Toggle", "", "Command Search", "Open the command search window", Key = Key.K, Modifiers = ModifierKeys.Control)]
    public void ToggleSearchWindow(string searchTerm)
    {
        SearchWindowOpen = !SearchWindowOpen;
        if (SearchWindowOpen)
        {
            SearchTerm = searchTerm;
        }
    }
}
