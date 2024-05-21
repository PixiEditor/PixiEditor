using System.Collections.Generic;
using System.Linq;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.AvaloniaUI.Models.Commands.Commands;
using PixiEditor.AvaloniaUI.Models.Commands.Evaluators;
using PixiEditor.AvaloniaUI.Models.Input;
using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.AvaloniaUI.Models.Commands;

internal class CommandGroup : ObservableObject
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

    public string? IsVisibleProperty { get; set; }

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
