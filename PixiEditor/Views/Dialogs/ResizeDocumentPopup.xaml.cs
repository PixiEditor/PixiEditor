using System.Windows;
using System.Windows.Input;

namespace PixiEditor.Views
{
    /// <summary>
    ///     Interaction logic for ResizeDocumentPopup.xaml
    /// </summary>
    public partial class ResizeDocumentPopup : ResizeablePopup
    {
        public ResizeDocumentPopup()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            DataContext = this;
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
