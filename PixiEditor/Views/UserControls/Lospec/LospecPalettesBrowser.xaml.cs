using PixiEditor.Models.DataHolders;
using PixiEditor.Models.ExternalServices;
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

namespace PixiEditor.Views.UserControls.Lospec
{
    /// <summary>
    /// Interaction logic for LospecPalettesBrowser.xaml
    /// </summary>
    public partial class LospecPalettesBrowser : UserControl
    {
        public PaletteList PaletteList
        {
            get { return (PaletteList)GetValue(PaletteListProperty); }
            set { SetValue(PaletteListProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PaletteList.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PaletteListProperty =
            DependencyProperty.Register("PaletteList", typeof(PaletteList), typeof(LospecPalettesBrowser));


        public LospecPalettesBrowser()
        {
            InitializeComponent();
        }

        private async void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            if(PaletteList == null)
                PaletteList = await LospecPaletteFetcher.FetchPage(0);
        }
    }
}
