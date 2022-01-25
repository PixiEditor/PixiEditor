using PixiEditor.Models.Controllers.Shortcuts;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace PixiEditor.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for ShortcutPopup.xaml.
    /// </summary>
    public partial class ShortcutPopup : Window
    {
        public static readonly DependencyProperty ControllerProperty =
            DependencyProperty.Register(nameof(Controller), typeof(ShortcutController), typeof(ShortcutPopup));

        public ShortcutController Controller { get => (ShortcutController)GetValue(ControllerProperty); set => SetValue(ControllerProperty, value); }

        public static readonly DependencyProperty IsTopmostProperty =
            DependencyProperty.Register(nameof(IsTopmost), typeof(bool), typeof(ShortcutPopup));

        public bool IsTopmost { get => (bool)GetValue(IsTopmostProperty); set => SetValue(IsTopmostProperty, value); }

        public ShortcutPopup(ShortcutController controller)
        {
            DataContext = this;
            InitializeComponent();
            Controller = controller;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;

            Hide();
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void CommandBinding_Executed_Maximize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        private void CommandBinding_Executed_Restore(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }
    }
}
