using System.Windows.Controls;
using PixiEditor.Models.Controllers.Shortcuts;

namespace PixiEditor.Helpers.UI
{
    public class UniversalTextBox : TextBox
    {
        public UniversalTextBox()
            : base()
        {
            GotFocus += UniversalTextBox_GotFocus;
            LostFocus += UniversalTextBox_LostFocus;
        }

        private void UniversalTextBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            ShortcutController.BlockShortcutExecution = true;
        }

        private void UniversalTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            ShortcutController.BlockShortcutExecution = false;
        }
    }
}