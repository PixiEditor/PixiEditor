using PixiEditor.Models.Commands.Commands;

namespace PixiEditor.Models.Commands.CommandLog;

internal class CommandLogEntry
{
    public Command Command { get; }

    public bool? CanExecute { get; set; }

    public DateTime DateTime { get; }

    public CommandLogEntry(Command command, bool? commandMethod, DateTime dateTime)
    {
        Command = command;
        CanExecute = commandMethod;
        DateTime = dateTime;
    }
}
