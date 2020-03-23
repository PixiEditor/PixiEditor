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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PixiEditor.Views
{
    /// <summary>
    /// Interaction logic for NewFilePopup.xaml
    /// </summary>
    public partial class NewFilePopup : Window
    {
        public NewFilePopup()
        {
            InitializeComponent();
        }



        public int FileHeight
        {
            get { return (int)GetValue(FileHeightProperty); }
            set { SetValue(FileHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FileHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FileHeightProperty =
            DependencyProperty.Register("FileHeight", typeof(int), typeof(NewFilePopup), new PropertyMetadata(16));



        public int FileWidth
        {
            get { return (int)GetValue(FileWidthProperty); }
            set { SetValue(FileWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FileWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FileWidthProperty =
            DependencyProperty.Register("FileWidth", typeof(int), typeof(NewFilePopup), new PropertyMetadata(16));




    }
}
