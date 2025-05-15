using PixiEditor.Extensions.CommonApi.Commands;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Commands.Commands;
using PixiEditor.Models.Commands.Evaluators;
using PixiEditor.Models.Input;
using Key = Avalonia.Input.Key;
using KeyModifiers = Avalonia.Input.KeyModifiers;
using Shortcut = PixiEditor.Extensions.CommonApi.Commands.Shortcut;

namespace PixiEditor.Models.ExtensionServices;

public class CommandProvider : ICommandProvider
{
    private IIconLookupProvider _iconLookupProvider;

    public CommandProvider(IIconLookupProvider iconLookupProvider)
    {
        _iconLookupProvider = iconLookupProvider;
    }
    public void RegisterCommand(CommandMetadata command, Action execute, Func<bool>? canExecute = null)
    {
        CanExecuteEvaluator evaluator = CanExecuteEvaluator.AlwaysTrue;

        if (canExecute != null)
        {
            evaluator = new CanExecuteEvaluator
            {
                Evaluate = _ => canExecute(),
                Name = $"{command.UniqueName}._canExecute"
            };

            CommandController.Current.CanExecuteEvaluators[evaluator.Name] = evaluator;
        }

        var shortcut = ToKeyCombination(command.Shortcut);
        Command.BasicCommand basicCommand = new Command.BasicCommand(_ => execute(), evaluator)
        {
            InternalName = command.UniqueName,
            MenuItemPath = command.MenuItemPath,
            Icon = LookupIcon(command.Icon),
            DisplayName = command.DisplayName,
            Description = command.Description,
            MenuItemOrder = command.Order,
            DefaultShortcut = shortcut,
            InvokePermissions = (CommandPermissions)command.InvokePermissions,
            ExplicitPermissions = command.ExplicitlyAllowedExtensions?.Split(';'),
            IconEvaluator = IconEvaluator.Default
        };

        CommandController.Current.AddManagedCommand(basicCommand);
    }

    public void InvokeCommand(string commandName)
    {
        if (CommandController.Current.Commands.ContainsKey(commandName))
        {
            var command = CommandController.Current.Commands[commandName];
            if (command.CanExecute())
            {
                command.Execute();
            }
        }
    }

    public bool CommandExists(string commandName)
    {
        return CommandController.Current.Commands.ContainsKey(commandName);
    }

    private static KeyCombination ToKeyCombination(Shortcut? shortcut)
    {
        if (shortcut is null or { Key: 0, Modifiers: 0 })
            return KeyCombination.None;

        return new KeyCombination((Key)shortcut.Key, (KeyModifiers)shortcut.Modifiers);
    }

    private string LookupIcon(string icon)
    {
        if (string.IsNullOrEmpty(icon))
            return string.Empty;

        return _iconLookupProvider.LookupIcon(icon) ?? icon;
    }
}
