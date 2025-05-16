using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Commands.Commands;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Input;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Views.Shortcuts;

internal class ShortcutBox : ContentControl
{
    private readonly KeyCombinationBox box;
    private bool changingCombination;

    public static readonly StyledProperty<Command> CommandProperty = AvaloniaProperty.Register<ShortcutBox, Command>(
        nameof(Command));

    public Command Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    static ShortcutBox()
    {
        CommandProperty.Changed.Subscribe(CommandUpdated);
    }

    public ShortcutBox()
    {
        Content = box = new KeyCombinationBox();
        box.KeyCombinationChanged += Box_KeyCombinationChanged;
    }

    private void Command_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Command.Shortcut))
        {
            UpdateBoxCombination();
        }
    }

    private void Box_KeyCombinationChanged(object sender, KeyCombination e)
    {
        if (changingCombination)
        {
            return;
        }

        changingCombination = true;
        var controller = CommandController.Current;

        if (e != KeyCombination.None)
        {
            if (controller.Commands[e].Where(x => x.ShortcutContexts == null || x.ShortcutContexts == Command.ShortcutContexts)
                    .SkipWhile(x => x == Command).FirstOrDefault() is { } oldCommand)
            {
                var oldShortcut = Command.Shortcut;
                bool enableSwap = oldShortcut is not { Key: Key.None, Modifiers: KeyModifiers.None };

                LocalizedString text = enableSwap ?
                    new LocalizedString("SHORTCUT_ALREADY_ASSIGNED_SWAP", oldCommand.DisplayName) :
                    new LocalizedString("SHORTCUT_ALREADY_ASSIGNED_OVERWRITE", oldCommand.DisplayName);
                OptionsDialog<string> dialog = new OptionsDialog<string>("ALREADY_ASSIGNED", text, MainWindow.Current);

                dialog.Add(new LocalizedString("REPLACE"), x => controller.ReplaceShortcut(Command, e));
                if (enableSwap)
                {
                    dialog.Add(new LocalizedString("SWAP"), x =>
                    {
                        controller.ReplaceShortcut(Command, e);
                        controller.ReplaceShortcut(oldCommand, oldShortcut);
                    });
                }
                dialog.Add(new LocalizedString("CANCEL"), x => box.KeyCombination = Command.Shortcut);

                dialog.ShowDialog();
                changingCombination = false;
                return;
            }
        }

        changingCombination = false;
        controller.UpdateShortcut(Command, e);
    }

    private void UpdateBoxCombination()
    {
        changingCombination = true;
        box.KeyCombination = Command?.Shortcut ?? default;
        box.DefaultCombination = Command?.DefaultShortcut ?? default;
        changingCombination = false;
    }

    private static void CommandUpdated(AvaloniaPropertyChangedEventArgs<Command> e)
    {
        var box = e.Sender as ShortcutBox;

        if (e.OldValue.Value is { } oldValue)
        {
            oldValue.PropertyChanged -= box.Command_PropertyChanged;
        }

        if (e.NewValue.Value is { } newValue)
        {
            newValue.PropertyChanged += box.Command_PropertyChanged;
        }

        box.UpdateBoxCombination();
    }
}
