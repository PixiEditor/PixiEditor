using System.Collections.Generic;
using System.Globalization;
using System.Text;
using PixiEditor.Models.Commands.Commands;

namespace PixiEditor.Models.Commands.CommandLog;

internal class CommandLog
{
    private readonly List<CommandLogEntry> list = new(MaxEntries);

    private const int MaxEntries = 8;

    public void Log(Command command, bool? canExecute)
    {
        if (canExecute.HasValue && !list[0].CanExecute.HasValue)
        {
            list[0].CanExecute = canExecute;
            return;
        }

        if (list.Count >= MaxEntries)
        {
            list.RemoveRange(MaxEntries - 1, list.Count - MaxEntries + 1);
        }

        list.Insert(0, new CommandLogEntry(command, canExecute, DateTime.Now));
    }

    public string GetSummary(DateTime relativeTime)
    {
        var builder = new StringBuilder();

        foreach (var entry in list)
        {
            var relativeSpan = entry.DateTime - relativeTime;
            string canExecute = entry.CanExecute.HasValue ? entry.CanExecute.ToString()! : "not executed";

            builder.AppendLine($"{entry.Command.InternalName} | CanExecute: {canExecute} | {relativeSpan.TotalSeconds.ToString("F3", CultureInfo.InvariantCulture)}s ago | {entry.DateTime.ToString("O", CultureInfo.InvariantCulture)}");
        }

        return builder.ToString();
    }
}
