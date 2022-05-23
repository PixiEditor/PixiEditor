using PixiEditor.Helpers;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Commands.Search;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.IO;
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

            var filePath = SearchTerm.Trim(' ', '"', '\'');

            if (filePath.StartsWith("~"))
            {
                filePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), filePath[1..]);
            }

            HandleFile(filePath);

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
                         .OrderByDescending(x =>
                             x.Description.Contains($" {SearchTerm} ", StringComparison.OrdinalIgnoreCase))
                         .Take(12))
            {
                Results.Add(
                    new CommandSearchResult(command)
                    {
                        SearchTerm = searchTerm,
                        Match = Match(command.Description)
                    });
            }

            foreach (var file in ViewModelMain.Current.FileSubViewModel.RecentlyOpened
                         .Where(x => x.FilePath.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
            {
                Results.Add(
                    new FileSearchResult(file.FilePath)
                    {
                        SearchTerm = searchTerm,
                        Match = Match(file.FilePath)
                    });
            }

            SelectedResult = Results.FirstOrDefault(x => x.CanExecute);
        }

        private void HandleFile(string filePath)
        {
            if (!Path.IsPathFullyQualified(filePath))
            {
                return;
            }

            GetDirectory(filePath, out var directory, out var name);
            var files = Directory.EnumerateFiles(directory)
                .Where(x => SupportedFilesHelper.IsExtensionSupported(Path.GetExtension(x)));

            if (name is not null or "")
            {
                files = files.Where(x => x.Contains(name, StringComparison.OrdinalIgnoreCase));
            }

            foreach (var file in files.Select(x => Path.GetFullPath(x)))
            {
                Results.Add(
                    new FileSearchResult(file)
                    {
                        SearchTerm = name,
                        Match = Match($".../{Path.GetFileName(file)}", name)
                    });
            }
        }

        private Match Match(string text) => Match(text, SearchTerm);

        private Match Match(string text, string searchTerm) =>
            Regex.Match(text, $"(.*)({Regex.Escape(searchTerm)})(.*)", RegexOptions.IgnoreCase);

        private bool GetDirectory(string path, out string directory, out string file)
        {
            if (Directory.Exists(path))
            {
                directory = path;
                file = string.Empty;
                return true;
            }

            directory = Path.GetDirectoryName(path);
            file = Path.GetFileName(path);

            return Directory.Exists(directory);
        }
    }
}