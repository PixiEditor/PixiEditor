using System.Windows;
using System.Windows.Input;

namespace PixiEditor.Views
{
    /// <summary>
    ///     Interaction logic for ResizeDocumentPopup.xaml
    /// </summary>
    public partial class ResizeDocumentPopup : Window
    {
        // Using a DependencyProperty as the backing store for NewHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NewHeightProperty =
            DependencyProperty.Register("NewHeight", typeof(int), typeof(ResizeDocumentPopup), new PropertyMetadata(0));

        // Using a DependencyProperty as the backing store for NewWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NewWidthProperty =
            DependencyProperty.Register("NewWidth", typeof(int), typeof(ResizeDocumentPopup), new PropertyMetadata(0));

        public ResizeDocumentPopup()
        {
            InitializeComponent();
            DataContext = this;
        }

        public int NewHeight
        {
            get => (int) GetValue(NewHeightProperty);
            set => SetValue(NewHeightProperty, value);
        }


        public int NewWidth
        {
            get => (int) GetValue(NewWidthProperty);
            set => SetValue(NewWidthProperty, value);
        }


        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}