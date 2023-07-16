using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using PixiEditor.Models.DataHolders;
using ReactiveUI;

namespace PixiEditor.Models.Commands.Search;

internal abstract class SearchResult : ReactiveObject
{
    private bool isSelected;
    private bool isMouseSelected;

    public string SearchTerm { get; init; }

    public virtual Inline[] TextBlockContent => GetInlines().ToArray();

    public Match Match { get; init; }

    public abstract string Text { get; }

    public virtual AvaloniaObject Description { get; }

    public abstract bool CanExecute { get; }

    public abstract IImage Icon { get; }

    public bool IsSelected
    {
        get => isSelected;
        set => this.RaiseAndSetIfChanged(ref isSelected, value);
    }

    public bool IsMouseSelected
    {
        get => isMouseSelected;
        set => this.RaiseAndSetIfChanged(ref isMouseSelected, value);
    }


    public abstract Task Execute();

    public virtual KeyCombination Shortcut { get; }

    public IReactiveCommand ExecuteCommand { get; }

    public SearchResult()
    {
        ExecuteCommand = ReactiveCommand.CreateFromTask(_ => Execute(), this.WhenAnyValue(x => CanExecute));
    }

    private IEnumerable<Inline> GetInlines()
    {
        if (Match is not { Success: true })
        {
            yield return new Run(Text);
            yield break;
        }

        foreach (Group group in Match.Groups.Values.Skip(1))
        {
            var run = new Run(group.Value);

            if (group.Value.Equals(SearchTerm, StringComparison.OrdinalIgnoreCase))
            {
                run.FontWeight = FontWeight.Bold;
            }

            yield return run;
        }
    }
}
