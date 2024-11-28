using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Drawie.Backend.Core.ColorsImpl;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Commands.Search;
using PixiEditor.ViewModels;
using CommandSearchResult = PixiEditor.Models.Commands.Search.CommandSearchResult;
using Search_CommandSearchResult = PixiEditor.Models.Commands.Search.CommandSearchResult;

namespace PixiEditor.Views.Main.CommandSearch;

#nullable enable
internal static class CommandSearchControlHelper
{
    public static (List<SearchResult> results, List<string> warnings) ConstructSearchResults(string query)
    {
        // avoid xaml designer error
        if (ViewModelMain.Current is null)
            return (new(), new());

        List<SearchResult> newResults = new();
        List<string> warnings = new();

        if (string.IsNullOrWhiteSpace(query))
        {
            // show all recently opened
            newResults.AddRange(ViewModelMain.Current.FileSubViewModel.RecentlyOpened
                .Select(file => (SearchResult)new FileSearchResult(file.FilePath)
                {
                    SearchTerm = query
                }));
            return (newResults, warnings);
        }

        var controller = CommandController.Current;

        if (query.StartsWith(':') && query.Length > 1)
        {
            string searchTerm = query[1..].Replace(" ", "");
            int index = searchTerm.IndexOf(':');

            string menu;
            string additional;
            
            if (index > 0)
            {
                menu = searchTerm[..index];
                additional = searchTerm[(index + 1)..];
            }
            else
            {
                menu = searchTerm;
                additional = string.Empty;
            }

            var menuCommands = controller.FilterCommands
                .Where(x => x.Key.Replace(" ", "").Contains(menu, StringComparison.OrdinalIgnoreCase))
                .SelectMany(x => x.Value);

            newResults.AddRange(menuCommands
                .Where(x => index == -1 || x.DisplayName.Value.Replace(" ", "").Contains(additional, StringComparison.OrdinalIgnoreCase))
                .Select(command => new Search_CommandSearchResult(command) { SearchTerm = searchTerm }));

            return (newResults, warnings);
        }
        
        // add matching colors
        MaybeParseColor(query).Switch(
            color =>
            {
                newResults.Add(new ColorSearchResult(color)
                {
                    SearchTerm = query
                });
                newResults.Add(ColorSearchResult.PastePalette(color, query));
            },
            (Error _) => warnings.Add("Invalid color"),
            static (None _) => { }
            );

        // add matching commands
        newResults.AddRange(
            controller.Commands
                .Where(x => x.Description.Value.Replace(" ", "").Contains(query.Replace(" ", ""), StringComparison.OrdinalIgnoreCase))
                .Where(static x => ViewModelMain.Current.DebugSubViewModel.UseDebug ? true : !x.IsDebug)
                .OrderByDescending(x => x.Description.Value.Contains($" {query} ", StringComparison.OrdinalIgnoreCase))
                .Take(18)
                .Select(command => new Search_CommandSearchResult(command)
                {
                    SearchTerm = query,
                    Match = Match(command.Description, query)
                }));

        try
        {
            // add matching files
            newResults.AddRange(MaybeParseFilePaths(query, warnings));
        }
        catch
        {
            // ignored
        }

        if (!query.StartsWith('.'))
        {
            // add matching recent files
            newResults.AddRange(
                ViewModelMain.Current.FileSubViewModel.RecentlyOpened
                    .Where(x => x.FilePath.Contains(query))
                    .Select(file => new FileSearchResult(file.FilePath)
                    {
                        SearchTerm = query, Match = Match(file.FilePath, query)
                    }));
        }

        return (newResults, warnings);
    }

    private static Match Match(string text, string searchTerm) =>
            Regex.Match(text, $"(.*)({Regex.Escape(searchTerm ?? string.Empty)})(.*)", RegexOptions.IgnoreCase);

    private static IEnumerable<SearchResult> MaybeParseFilePaths(string query, List<string> warnings)
    {
        var filePath = query.Trim(' ', '"', '\'');

        if (filePath.StartsWith('~'))
            filePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), filePath[1..]);

        if (filePath.StartsWith('.') && (filePath.Length == 1 || filePath[1] != '.'))
        {
            if (ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument is { FullFilePath: { } path })
            {
                int skip = 1;
                
                path = Path.GetDirectoryName(path);
                
                if (filePath.Length > 1 && filePath[1] == '.')
                {
                    skip++;
                    path = Path.GetDirectoryName(path);
                }
                
                filePath = Path.Join(path, filePath[skip..]);
            }
            else
            {
                warnings.Add("Save current document to browse files");
            }
        }
        
        if (!Path.IsPathFullyQualified(filePath))
            return Enumerable.Empty<SearchResult>();

        GetDirectory(filePath, out var directory, out var name);
        var files = Directory.EnumerateFiles(directory)
            .Where(x => SupportedFilesHelper.IsExtensionSupported(Path.GetExtension(x)));

        if (!files.Any())
        {
            warnings.Add($"Directory '{Path.GetFullPath(filePath).TrimEnd(Path.DirectorySeparatorChar)}' does not have any files.");
            return Enumerable.Empty<SearchResult>();
        }

        if (name is not (null or ""))
        {
            files = files.Where(x => x.Contains(name, StringComparison.OrdinalIgnoreCase));
        }

        string[] array = files as string[] ?? files.ToArray();
        
        if (array.Length != 1)
        {
            return array
                .Select(static file => Path.GetFullPath(file))
                .Select(path => new FileSearchResult(path)
                {
                    SearchTerm = name, Match = Match($".../{Path.GetFileName(path)}", name ?? "")
                });
        }

        return array.Length >= 1 ? new[] { new FileSearchResult(array[0]), new FileSearchResult(array[0], true) } : ArraySegment<SearchResult>.Empty;
    }

    private static bool GetDirectory(string path, out string directory, out string file)
    {
        if (Directory.Exists(path))
        {
            directory = path;
            file = string.Empty;
            return true;
        }

        directory = Path.GetDirectoryName(path) ?? @"C:\";
        file = Path.GetFileName(path);

        return Directory.Exists(directory);
    }

    public static OneOf<Color, Error, None> MaybeParseColor(string query)
    {
        if (query.StartsWith('#'))
        {
            if (!Color.TryParse(query, out var color))
                return new Error();
            return color;
        }
        else if (query.StartsWith("rgb") || query.StartsWith("rgba"))
        {
            // matches strings that:
            // - start with "rgb" or "rgba"
            // - have a list of 3 or 4 numbers each up to 255 (rgb with 4 numbers is still allowed)
            // - can have parenteses around the list
            // - can have spaces in any reasonable places
            Match match = Regex.Match(query, @"^rgba? *(?(?=\()\((?=.+\))|(?!.+\))) *(?<r>(?:1?\d{1,2})|(?:2[1-4]\d)|(?:25[1-5])) *, *(?<g>(?:1?\d{1,2})|(?:2[1-4]\d)|(?:25[1-5])) *, *(?<b>(?:1?\d{1,2})|(?:2[1-4]\d)|(?:25[1-5])) *(?:, *(?<a>(?:1?\d{1,2})|(?:2[1-4]\d)|(?:25[1-5])))?\)?$");

            if (match.Success)
            {
                var maybeColor = ParseRGB(match);
                return maybeColor is null ? new Error() : maybeColor.Value;
            }
            else if (query.StartsWith("rgb(") || query.StartsWith("rgba("))
            {
                return new Error();
            }
        }
        return new None();
    }

    private static Color? ParseRGB(Match match)
    {
        bool invalid = !(
            byte.TryParse(match.Groups["r"].ValueSpan, out var r) &
            byte.TryParse(match.Groups["g"].ValueSpan, out var g) &
            byte.TryParse(match.Groups["b"].ValueSpan, out var b)
        );

        if (invalid)
            return null;

        var aText = match.Groups["a"].Value;
        byte a = 255;
        if (!string.IsNullOrEmpty(aText) && !byte.TryParse(aText, out a))
            return null;

        return new Color(r, g, b, a);
    }
}
