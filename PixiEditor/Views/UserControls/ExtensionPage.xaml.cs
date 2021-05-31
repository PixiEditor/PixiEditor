using PixiEditor.SDK;
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
    /// Interaction logic for ExtensionPage.xaml
    /// </summary>
    public partial class ExtensionPage : UserControl
    {
        //public static readonly DependencyProperty ExtensionProperty =
        //    DependencyProperty.Register(nameof(Extension), typeof(Extension), typeof(ExtensionPage), new PropertyMetadata(ExtensionChanged));

        //public Extension Extension { get => (Extension)GetValue(ExtensionProperty); set => SetValue(ExtensionProperty, value); }

        public ExtensionPage()
        {
            InitializeComponent();
        }

        private static void ExtensionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ExtensionPage c = d as ExtensionPage;
            c.DataContext = e.NewValue;
        }
    }
}
