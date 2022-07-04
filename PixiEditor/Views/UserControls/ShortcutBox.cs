using PixiEditor.Models.Commands;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Dialogs;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PixiEditor.Views.UserControls
{
    public class ShortcutBox : ContentControl
    {
        private readonly KeyCombinationBox box;
        private bool changingCombination;

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(Command), typeof(ShortcutBox), new PropertyMetadata(null, CommandUpdated));

        public Command Command
        {
            get => (Command)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
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
                if (controller.Commands[e].SkipWhile(x => x == Command).FirstOrDefault() is { } oldCommand)
                {
                    var oldShortcut = Command.Shortcut;
                    bool enableSwap = oldShortcut is not { Key: Key.None, Modifiers: ModifierKeys.None };
                    
                    string text = enableSwap ?
                        $"This shortcut is already assigned to '{oldCommand.DisplayName}'\nDo you want to replace the existing shortcut or swap the two?" :
                        $"This shortcut is already assigned to '{oldCommand.DisplayName}'\nDo you want to replace the existing shortcut?";
                    OptionsDialog<string> dialog = new("Already assigned", text);
                    
                    dialog.Add("Replace", x => controller.ReplaceShortcut(Command, e));
                    if (enableSwap)
                    {
                        dialog.Add("Swap", x =>
                        {
                            controller.ReplaceShortcut(Command, e);
                            controller.ReplaceShortcut(oldCommand, oldShortcut);
                        });
                    }
                    dialog.Add("Cancel", x => box.KeyCombination = Command.Shortcut);

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

        private static void CommandUpdated(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var box = dp as ShortcutBox;

            if (e.OldValue is Command oldValue)
            {
                oldValue.PropertyChanged -= box.Command_PropertyChanged;
            }

            if (e.NewValue is Command newValue)
            {
                newValue.PropertyChanged += box.Command_PropertyChanged;
            }

            box.UpdateBoxCombination();
        }
    }
}
