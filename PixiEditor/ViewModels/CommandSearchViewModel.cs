using PixiEditor.Helpers;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Commands.Search;
using SkiaSharp;
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
            set
            {
                if (SetProperty(ref selectedCommand, value, out var oldValue))
                {
                    if (oldValue != null)
                    {
                        oldValue.IsSelected = false;
                    }
                    if (value != null)
                    {
                        value.IsSelected = true;
                    }
                }
            }
        }

        public ObservableCollection<SearchResult> Results { get; } = new();

        public CommandSearchViewModel()
        {
            UpdateSearchResults();
        }

        private void UpdateSearchResults()
        {
            CommandController controller = CommandController.Current;
            Results.Clear();

            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                foreach (var file in ViewModelMain.Current.FileSubViewModel.RecentlyOpened)
                {
                    Results.Add(
                        new FileSearchResult(file.FilePath)
                        {
                            SearchTerm = searchTerm
                        });
                }

                SelectedResult = Results.FirstOrDefault(x => x.CanExecute);
                return;
            }

            if (SearchTerm.StartsWith('#'))
            {
                if (SKColor.TryParse(SearchTerm, out SKColor color))
                {
                    Results.Add(new ColorSearchResult(color)
                    {
                        SearchTerm = searchTerm
                    });
                }
            }

            foreach (var command in controller.Commands
                .Where(x => x.Description.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.Description.Contains($" {SearchTerm} ", StringComparison.OrdinalIgnoreCase))
                .Take(12))
            {
                Results.Add(
                    new CommandSearchResult(command)
                    {
                        SearchTerm = searchTerm,
                        Match = Match(command.Description)
                    });
            }

            foreach (var file in ViewModelMain.Current.FileSubViewModel.RecentlyOpened.Where(x => x.FilePath.Contains(searchTerm)))
            {
                Results.Add(
                    new FileSearchResult(file.FilePath)
                    {
                        SearchTerm = searchTerm,
                        Match = Match(file.FilePath)
                    });
            }

            Match Match(string text) => Regex.Match(text, $"(.*)({Regex.Escape(SearchTerm)})(.*)", RegexOptions.IgnoreCase);

            SelectedResult = Results.FirstOrDefault(x => x.CanExecute);
        }
    }
}
