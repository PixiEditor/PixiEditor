using System.Collections;
using System.ComponentModel;
using System.Windows.Input;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Commands.Commands;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Localization;

namespace PixiEditor.Models.Commands;

internal class CommandGroup : NotifyableObject
{
    private readonly Command[] commands;
    private readonly Command[] visibleCommands;

    private LocalizedString displayName;

    public LocalizedString DisplayName
    {
        get => displayName;
        set => SetProperty(ref displayName, value);
    }

    public bool HasAssignedShortcuts { get; set; }

    public IEnumerable<Command> Commands => commands;

    public IEnumerable<Command> VisibleCommands => visibleCommands;

    public CommandGroup(LocalizedString displayName, IEnumerable<Command> commands)
    {
        DisplayName = displayName;
        this.commands = commands.ToArray();
        visibleCommands = commands.Where(x => !string.IsNullOrEmpty(x.DisplayName.Value)).ToArray();

        foreach (var command in commands)
        {
            HasAssignedShortcuts |= command.Shortcut.Key != Key.None;
            command.ShortcutChanged += Command_ShortcutChanged;
        }

        ILocalizationProvider.Current.OnLanguageChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged(Language obj)
    {
        DisplayName = new LocalizedString(DisplayName.Key);
    }

    private void Command_ShortcutChanged(Command cmd, ShortcutChangedEventArgs args)
    {
        if ((args.NewShortcut != KeyCombination.None && HasAssignedShortcuts) ||
            (args.NewShortcut == KeyCombination.None && !HasAssignedShortcuts))
        {
            // If a shortcut is already assigned and the new shortcut is not none nothing can change
            // If no shortcut is already assigned and the new shortcut is none nothing can change
            return;
        }

        HasAssignedShortcuts = false;

        foreach (var command in commands)
        {
            HasAssignedShortcuts |= command.Shortcut.Key != Key.None;
        }
    }

    public IEnumerator<Command> GetEnumerator() => Commands.GetEnumerator();
}
