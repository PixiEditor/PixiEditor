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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PixiEditor.Views.UserControls
{
    /// <summary>
    /// Interaction logic for LayerFolder.xaml.
    /// </summary>
    public partial class LayerFolder : UserControl
    {

        public string FolderName
        {
            get { return (string)GetValue(FolderNameProperty); }
            set { SetValue(FolderNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FolderName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FolderNameProperty =
            DependencyProperty.Register("FolderName", typeof(string), typeof(LayerFolder), new PropertyMetadata(""));

        public LayerFolder()
        {
            InitializeComponent();
        }

        private void FolderControl_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        private void FolderControl_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void Grid_DragEnter(object sender, DragEventArgs e)
        {

        }

        private void Grid_DragLeave(object sender, DragEventArgs e)
        {

        }

        private void Grid_Drop_Top(object sender, DragEventArgs e)
        {

        }

        private void Grid_Drop_Bottom(object sender, DragEventArgs e)
        {

        }
    }
}