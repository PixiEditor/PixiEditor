using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Models.Input;

namespace PixiEditor.Models.Commands.Search;

[DebuggerDisplay("{Text}, Can Execute = {CanExecute}")]
internal abstract class SearchResult : ObservableObject
{
    private bool isSelected;
    private bool isMouseSelected;

    public string SearchTerm { get; init; }
    
    public int Index { get; init; }

    public virtual Inline[] TextBlockContent => GetInlines().ToArray();

    public Match Match { get; init; }

    public abstract string Text { get; }

    public virtual AvaloniaObject Description { get; }

    public abstract bool CanExecute { get; }

    public abstract IImage Icon { get; }

    public bool IsSelected
    {
        get => isSelected;
        set => SetProperty(ref isSelected, value);
    }

    public bool IsMouseSelected
    {
        get => isMouseSelected;
        set => SetProperty(ref isMouseSelected, value);
    }

    public abstract void Execute();

    public virtual KeyCombination Shortcut { get; }

    public ICommand ExecuteCommand { get; }

    public SearchResult()
    {
        ExecuteCommand = new RelayCommand(Execute, () => CanExecute);
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
