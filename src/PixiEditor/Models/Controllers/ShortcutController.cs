using Avalonia.Input;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Commands.CommandContext;
using PixiEditor.Models.Commands.Commands;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Input;

namespace PixiEditor.Models.Controllers;

internal class ShortcutController
{
    public static bool ShortcutExecutionBlocked => _shortcutExecutionBlockers.Count > 0;

    private static readonly List<string> _shortcutExecutionBlockers = new List<string>();

    public IEnumerable<Command> LastCommands { get; private set; }

    public Type? ActiveContext { get; private set; }

    public static void BlockShortcutExecution(string blocker)
    {
        if (_shortcutExecutionBlockers.Contains(blocker)) return;
        _shortcutExecutionBlockers.Add(blocker);
    }

    public static void UnblockShortcutExecution(string blocker)
    {
        if (!_shortcutExecutionBlockers.Contains(blocker)) return;
        _shortcutExecutionBlockers.Remove(blocker);
    }

    public static void UnblockShortcutExecutionAll()
    {
        _shortcutExecutionBlockers.Clear();
    }

    public KeyCombination? GetToolShortcut<T>()
    {
        return GetToolShortcut(typeof(T));
    }

    public KeyCombination? GetToolShortcut(Type type)
    {
        return CommandController.Current.Commands.FirstOrDefault(x => x is Command.ToolCommand tool && tool.ToolType == type)?.Shortcut;
    }

    public KeyCombination? GetToolShortcut(IToolHandler tool)
    {
        return CommandController.Current.Commands.FirstOrDefault(x => x is Command.ToolCommand toolCmd && tool == toolCmd.Handler)?.Shortcut;
    }

    public void KeyPressed(bool isRepeat, Key key, KeyModifiers modifiers)
    {
        KeyCombination shortcut = new(key, modifiers);

        if (ShortcutExecutionBlocked)
        {
            return;
        }

        var commands = CommandController.Current.Commands[shortcut].Where(x => x.ShortcutContexts is null || x.ShortcutContexts.Contains(ActiveContext)).ToList();

        if (!commands.Any())
        {
            return;
        }

        LastCommands = commands;

        var context = ShortcutSourceInfo.GetContext(shortcut, isRepeat);
        foreach (var command in commands)
        {
            command.Execute(context, false);
        }
    }

    public void OverwriteContext(Type getType)
    {
        ActiveContext = getType;
    }
    
    public void ClearContext(Type clearFrom)
    {
        if (ActiveContext == clearFrom)
        {
            ActiveContext = null;
        }
    }
}
