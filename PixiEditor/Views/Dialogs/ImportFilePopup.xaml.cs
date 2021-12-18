using System.Windows;
using System.Windows.Input;
using PixiEditor.ViewModels;

namespace PixiEditor.Views
{
    /// <summary>
    ///     Interaction logic for ImportFilePopup.xaml
    /// </summary>
    public partial class ImportFilePopup : Window
    {
        private readonly ImportFilePopupViewModel dc = new ImportFilePopupViewModel();

        public ImportFilePopup()
        {
            InitializeComponent();
            DataContext = dc;
        }


        public int ImportHeight
        {
            get => dc.ImportHeight;
            set => dc.ImportWidth = value;
        }


        public int ImportWidth
        {
            get => dc.ImportWidth;
            set => dc.ImportWidth = value;
        }


        public string FilePath
        {
            get => dc.FilePath;
            set => dc.FilePath = value;
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }
    }
}