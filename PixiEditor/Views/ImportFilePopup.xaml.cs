using System.Windows;
using PixiEditor.ViewModels;

namespace PixiEditor.Views
{
    /// <summary>
    ///     Interaction logic for ImportFilePopup.xaml.
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
    }
}