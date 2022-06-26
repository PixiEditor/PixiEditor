using OneOf;
using OneOf.Types;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Commands.Search;
using PixiEditor.ViewModels;
using SkiaSharp;
using System.IO;
using System.Text.RegularExpressions;

namespace PixiEditor.Views.UserControls.CommandSearch;

#nullable enable
internal static class CommandSearchControlHelper
{
    public static (List<SearchResult> results, List<string> warnings) ConstructSearchResults(string query)
    {
        List<SearchResult> newResults = new();
        List<string> warnings = new();

        warnings.Add("haha warning");

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

        // add matching colors
        MaybeParseColor(query).Switch(
            (SKColor color) =>
            {
                newResults.Add(new ColorSearchResult(color)
                {
                    SearchTerm = query
                });
            },
            (Error _) => warnings.Add("Invalid color"),
            static (None _) => { }
            );

        // add matching commands
        newResults.AddRange(
            controller.Commands
                .Where(x => x.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.Description.Contains($" {query} ", StringComparison.OrdinalIgnoreCase))
                .Take(12)
                .Select(command => new CommandSearchResult(command)
                {
                    SearchTerm = query,
                    Match = Match(command.Description, query)
                }));

        // add matching files
        newResults.AddRange(MaybeParseFilePaths(query));

        // add matching recent files
        newResults.AddRange(
            ViewModelMain.Current.FileSubViewModel.RecentlyOpened
                .Where(x => x.FilePath.Contains(query))
                .Select(file => new FileSearchResult(file.FilePath)
                {
                    SearchTerm = query,
                    Match = Match(file.FilePath, query)
                }));

        return (newResults, warnings);
    }

    private static Match Match(string text, string searchTerm) =>
            Regex.Match(text, $"(.*)({Regex.Escape(searchTerm ?? string.Empty)})(.*)", RegexOptions.IgnoreCase);

    private static IEnumerable<SearchResult> MaybeParseFilePaths(string query)
    {
        var filePath = query.Trim(' ', '"', '\'');

        if (filePath.StartsWith("~"))
            filePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), filePath[1..]);

        if (!Path.IsPathFullyQualified(filePath))
            return Enumerable.Empty<SearchResult>();

        GetDirectory(filePath, out var directory, out var name);
        var files = Directory.EnumerateFiles(directory)
            .Where(x => SupportedFilesHelper.IsExtensionSupported(Path.GetExtension(x)));

        if (name is not (null or ""))
        {
            files = files.Where(x => x.Contains(name, StringComparison.OrdinalIgnoreCase));
        }

        return files
            .Select(static file => Path.GetFullPath(file))
            .Select(path => new FileSearchResult(path)
            {
                SearchTerm = name,
                Match = Match($".../{Path.GetFileName(path)}", name ?? "")
            });
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

    private static OneOf<SKColor, Error, None> MaybeParseColor(string query)
    {
        if (query.StartsWith('#'))
        {
            if (!SKColor.TryParse(query, out var color))
                return new Error();
            return color;
        }
        else if (query.StartsWith("rgb") || query.StartsWith("rgba"))
        {
            Match match = Regex.Match(query, @"rgba?\(? *(?<r>\d{1,3}) *, *(?<g>\d{1,3}) *, *(?<b>\d{1,3})(?: *, *(?<a>\d{1,3}))?");

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

    private static SKColor? ParseRGB(Match match)
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

        return new SKColor(r, g, b, a);
    }
}