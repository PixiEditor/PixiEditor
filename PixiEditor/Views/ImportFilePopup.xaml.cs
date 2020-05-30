using PixiEditor.ViewModels;
using System.Windows;

namespace PixiEditor.Views
{
    /// <summary>
    /// Interaction logic for ImportFilePopup.xaml
    /// </summary>
    public partial class ImportFilePopup : Window
    {
        ImportFilePopupViewModel dc = new ImportFilePopupViewModel();
        public ImportFilePopup()
        {
            InitializeComponent();
            this.DataContext = dc;
        }




        public int ImportHeight
        {
            get
            {
                return dc.ImportHeight;
            }
            set { dc.ImportWidth = value; }
        }



        public int ImportWidth
        {
            get { return dc.ImportWidth; }
            set { dc.ImportWidth = value; }
        }



        public string FilePath
        {
            get { return dc.FilePath; }
            set { dc.FilePath = value; }
        }
    }
}
