using System.Collections.Generic;
using System.Linq;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.Models.Commands.Evaluators;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Commands.Commands;
using PixiEditor.Models.Input;

namespace PixiEditor.Models.Commands;

internal class CommandGroup : ObservableObject
{
    private List<Command> commands;
    private List<Command> visibleCommands;

    private LocalizedString displayName;

    public LocalizedString DisplayName
    {
        get => displayName;
        set => SetProperty(ref displayName, value);
    }

    public bool HasAssignedShortcuts { get; set; }

    public string? IsVisibleProperty { get; set; }

    public IReadOnlyList<Command> Commands => commands;

    public IReadOnlyList<Command> VisibleCommands => visibleCommands;

    public CommandGroup(LocalizedString displayName, IEnumerable<Command> commands)
    {
        DisplayName = displayName;
        this.commands = commands.ToList();
        visibleCommands = commands.Where(x => !string.IsNullOrEmpty(x.DisplayName.Value)).ToList();

        foreach (var command in commands)
        {
            HasAssignedShortcuts |= command.Shortcut.Key != Key.None;
            command.ShortcutChanged += Command_ShortcutChanged;
        }

        ILocalizationProvider.Current.OnLanguageChanged += OnLanguageChanged;
    }

    public void AddCommand(Command command)
    {
        command.ShortcutChanged += Command_ShortcutChanged;
        HasAssignedShortcuts |= command.Shortcut.Key != Key.None;
        commands.Add(command);
        if (!string.IsNullOrEmpty(command.DisplayName.Value))
        {
            visibleCommands.Add(command);
        }

        OnPropertyChanged(nameof(VisibleCommands));
        OnPropertyChanged(nameof(Commands));
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
