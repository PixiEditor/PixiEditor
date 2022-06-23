using System.Text.RegularExpressions;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Commands.Search;
using PixiEditor.ViewModels;
using SkiaSharp;

namespace PixiEditor.Views.UserControls.CommandSearch;

#nullable enable
internal static class CommandSearchControlHelper
{
    public static List<SearchResult> ConstructSearchResults(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            // show all recently opened
            return ViewModelMain.Current.FileSubViewModel.RecentlyOpened
                .Select(file => (SearchResult)new FileSearchResult(file.FilePath)
                {
                    SearchTerm = query
                }).ToList();
        }

        var controller = CommandController.Current;
        List<SearchResult> newResults = new();

        // add matching colors
        if (query.StartsWith('#'))
        {
            if (SKColor.TryParse(query, out SKColor color))
            {
                newResults.Add(new ColorSearchResult(color)
                {
                    SearchTerm = query
                });
            }
        }

        // add matching commands
        newResults.AddRange(
            controller.Commands
                .Where(x => x.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.Description.Contains($" {query} ", StringComparison.OrdinalIgnoreCase))
                .Take(12)
                .Select(command => new CommandSearchResult(command)
                {
                    SearchTerm = query,
                    Match = Match(command.Description)
                }));

        // add matching recent files
        newResults.AddRange(
            ViewModelMain.Current.FileSubViewModel.RecentlyOpened
                .Where(x => x.FilePath.Contains(query))
                .Select(file => new FileSearchResult(file.FilePath)
                {
                    SearchTerm = query,
                    Match = Match(file.FilePath)
                }));

        Match Match(string text) => Regex.Match(text, $"(.*)({Regex.Escape(query)})(.*)", RegexOptions.IgnoreCase);

        return newResults;
    }
}