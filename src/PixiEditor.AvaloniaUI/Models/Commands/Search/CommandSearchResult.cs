using Avalonia.Media;
using PixiEditor.AvaloniaUI.Models.Commands.Commands;
using PixiEditor.AvaloniaUI.Models.Input;

namespace PixiEditor.AvaloniaUI.Models.Commands.Search;

internal class CommandSearchResult : SearchResult
{
    public Command Command { get; }

    public override string Text => Command.Description;

    public override bool CanExecute => Command.CanExecute();

    public override IImage Icon => Command.IconEvaluator.CallEvaluate(Command, this);

    public override KeyCombination Shortcut => Command.Shortcut;

    public CommandSearchResult(Command command) => Command = command;

    public override void Execute()
    {
        Command.Execute();
    }
}
