using Avalonia.Media;
using PixiEditor.Models.Commands.CommandContext;
using PixiEditor.Models.Commands.Commands;
using PixiEditor.Models.Input;

namespace PixiEditor.Models.Commands.Search;

internal class CommandSearchResult : SearchResult
{
    public Command Command { get; }

    public override string Text => Command.Description;

    public override bool CanExecute => Command.CanExecute();

    public override IImage Icon => Command.IconEvaluator.CallEvaluate(Command, this);

    public override KeyCombination Shortcut => Command.Shortcut;

    public CommandSearchResult(Command command)
    {
        Command = command;
        Command.CanExecuteChanged += () => OnPropertyChanged(nameof(CanExecute));
    }

    public override void Execute()
    {
        Command.Execute(SearchSourceInfo.GetContext(SearchTerm), false);
    }
}
