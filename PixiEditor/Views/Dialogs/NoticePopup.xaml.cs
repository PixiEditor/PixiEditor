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

namespace PixiEditor.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for NoticePopup.xaml.
    /// </summary>
    public partial class NoticePopup : Window
    {
        public static readonly DependencyProperty BodyProperty =
            DependencyProperty.Register(nameof(Body), typeof(string), typeof(NoticePopup));

        public string Body
        {
            get => (string)GetValue(BodyProperty);
            set => SetValue(BodyProperty, value);
        }

        public NoticePopup()
        {
            InitializeComponent();
        }

        private void OkButton_Close(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}