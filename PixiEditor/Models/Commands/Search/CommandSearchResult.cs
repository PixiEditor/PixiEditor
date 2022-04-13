using PixiEditor.Models.DataHolders;
using System.Windows.Media;

namespace PixiEditor.Models.Commands.Search
{
    public class CommandSearchResult : SearchResult
    {
        public Command Command { get; }

        public override string Text => Command.Description;

        public override bool CanExecute => Command.CanExecute();

        public override ImageSource Icon => Command.IconEvaluator.EvaluateEvaluator(Command, this);

        public override KeyCombination Shortcut => Command.Shortcut;

        public CommandSearchResult(Command command) => Command = command;

        public override void Execute() => Command.Execute();
    }
}
