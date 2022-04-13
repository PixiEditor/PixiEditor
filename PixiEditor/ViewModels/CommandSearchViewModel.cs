using PixiEditor.Helpers;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Commands.Search;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace PixiEditor.ViewModels
{
    public class CommandSearchViewModel : NotifyableObject
    {
        private string searchTerm;
        private SearchResult selectedCommand;

        public string SearchTerm
        {
            get => searchTerm;
            set
            {
                if (SetProperty(ref searchTerm, value))
                {
                    UpdateSearchResults();
                }
            }
        }

        public SearchResult SelectedResult
        {
            get => selectedCommand;
            set => SetProperty(ref selectedCommand, value);
        }

        public ObservableCollection<SearchResult> Commands { get; } = new();

        public CommandSearchViewModel()
        {
            UpdateSearchResults();
        }

        private void UpdateSearchResults()
        {
            CommandController controller = CommandController.Current;
            Commands.Clear();

            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                foreach (var file in ViewModelMain.Current.FileSubViewModel.RecentlyOpened)
                {
                    Commands.Add(
                        new FileSearchResult(file.FilePath)
                        {
                            SearchTerm = searchTerm
                        });
                }

                return;
            }

            foreach (var command in controller.Commands
                .Where(x => x.Description.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.Description.Contains($" {SearchTerm} ", StringComparison.OrdinalIgnoreCase))
                .Take(12))
            {
                Commands.Add(
                    new CommandSearchResult(command)
                    {
                        SearchTerm = searchTerm,
                        Match = Match(command.Description)
                    });
            }

            foreach (var file in ViewModelMain.Current.FileSubViewModel.RecentlyOpened.Where(x => x.FilePath.Contains(searchTerm)))
            {
                Commands.Add(
                    new FileSearchResult(file.FilePath)
                    {
                        SearchTerm = searchTerm,
                        Match = Match(file.FilePath)
                    });
            }

            Match Match(string text) => Regex.Match(text, $"(.*)({Regex.Escape(SearchTerm)})(.*)", RegexOptions.IgnoreCase);
        }
    }
}
