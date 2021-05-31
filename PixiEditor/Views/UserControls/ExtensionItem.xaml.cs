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
    /// Interaction logic for ExtensionItem.xaml
    /// </summary>
    public partial class ExtensionItem : UserControl
    {
        public static readonly DependencyProperty SelectedExtensionProperty =
            DependencyProperty.Register(nameof(SelectedExtension), typeof(Extension), typeof(ExtensionItem), new PropertyMetadata(SelectedExtensionChanged));

        public Extension SelectedExtension { get => (Extension)GetValue(SelectedExtensionProperty); set => SetValue(SelectedExtensionProperty, value); }

        public static readonly DependencyProperty ExtensionProperty =
            DependencyProperty.Register(nameof(Extension), typeof(Extension), typeof(ExtensionItem), new PropertyMetadata(SelectedExtensionChanged));

        public Extension Extension { get => (Extension)GetValue(ExtensionProperty); set => SetValue(ExtensionProperty, value); }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(ExtensionItem));

        public bool IsSelected { get => (bool)GetValue(IsSelectedProperty); set => SetValue(IsSelectedProperty, value); }

        public ExtensionItem()
        {
            InitializeComponent();
        }

        private static void SelectedExtensionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Extension extension = (Extension)d.GetValue(ExtensionProperty);

            d.SetValue(IsSelectedProperty, e.NewValue == extension);
        }
    }
}
