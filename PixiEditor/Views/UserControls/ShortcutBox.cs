using PixiEditor.Models.Commands;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Dialogs;
using System.Windows;
using System.Windows.Controls;

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
                if (controller.Commands[e].Any())
                {
                    OptionsDialog<string> dialog = new("Already assigned", $"This shortcut is already asigned to '{controller.Commands[e].First().Display}'\nDo you want to replace the shortcut or switch shortcuts?")
                    {
                        {
                            "Replace", x => controller.ReplaceShortcut(Command, e) 
                        },
                        {
                            "Switch", x =>
                            {
                                var oldCommand = controller.Commands[e].First();
                                var oldShortcut = Command.Shortcut;
                                controller.ReplaceShortcut(Command, e);
                                controller.ReplaceShortcut(oldCommand, oldShortcut);
                            }
                        },
                        {
                            "Abort", x =>
                            {
                                box.KeyCombination = Command.Shortcut;
                            }
                        }
                    };

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
