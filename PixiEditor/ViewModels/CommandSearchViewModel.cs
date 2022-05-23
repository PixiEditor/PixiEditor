using PixiEditor.Helpers;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Commands.Search;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using PixiEditor.Models.DataHolders;

namespace PixiEditor.ViewModels
{
    public class CommandSearchViewModel : NotifyableObject
    {
        private string searchTerm;
        private SearchResult selectedCommand;
        private bool invalidColor;

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

        public bool InvalidColor
        {
            get => invalidColor;
            set => SetProperty(ref invalidColor, value);
        }
        
        public ObservableCollection<SearchResult> Results { get; } = new();

        public CommandSearchViewModel()
        {
            UpdateSearchResults();
        }

        private void UpdateSearchResults()
        {
            Results.Clear();

            var recentlyOpened = HandleRecentlyOpened();
            
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                foreach (var result in recentlyOpened)
                {
                    Results.Add(result);
                }

                SelectedResult = Results.FirstOrDefault(x => x.CanExecute);
                InvalidColor = false;
                return;
            }

            var filePath = SearchTerm.Trim(' ', '"', '\'');

            if (filePath.StartsWith("~"))
            {
                filePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), filePath[1..]);
            }

            HandleFile(filePath);
            HandleColor();
            HandleCommands();

            foreach (var result in recentlyOpened)
            {
                Results.Add(result);
            }

            SelectedResult = Results.FirstOrDefault(x => x.CanExecute);
        }

        private void HandleColor()
        {
            if (SearchTerm.StartsWith('#'))
            {
                InvalidColor = !SKColor.TryParse(SearchTerm, out var color);
                
                if (!InvalidColor)
                {
                    Results.Add(new ColorSearchResult(color)
                    {
                        SearchTerm = searchTerm
                    });
                }
            }
            else
            {
                InvalidColor = false;
            }
        }

        private void HandleCommands()
        {
            foreach (var command in CommandController.Current.Commands
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
        }
        
        private IEnumerable<FileSearchResult> HandleRecentlyOpened()
        {
            IEnumerable<RecentlyOpenedDocument> enumerable = ViewModelMain.Current.FileSubViewModel.RecentlyOpened;

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                enumerable = enumerable.Where(x => x.FilePath.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            foreach (var file in enumerable)
            {
                yield return new FileSearchResult(file.FilePath)
                {
                    SearchTerm = searchTerm,
                    Match = Match(file.FilePath)
                };
            }
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
            Regex.Match(text, $"(.*)({Regex.Escape(searchTerm ?? string.Empty)})(.*)", RegexOptions.IgnoreCase);

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