using PixiEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
