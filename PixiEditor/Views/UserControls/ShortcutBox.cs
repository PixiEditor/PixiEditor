using PixiEditor.Models.Commands;
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

        private void Box_KeyCombinationChanged(object sender, Models.DataHolders.KeyCombination e)
        {
            if (!changingCombination)
            {
                CommandController.Current.UpdateShortcut(Command, e);
            }
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
