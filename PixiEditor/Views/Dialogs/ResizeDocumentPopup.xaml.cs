using System.Windows;
using System.Windows.Input;

namespace PixiEditor.Views
{
    /// <summary>
    ///     Interaction logic for ResizeDocumentPopup.xaml
    /// </summary>
    public partial class ResizeDocumentPopup : Window
    {
        public static readonly DependencyProperty NewHeightProperty =
            DependencyProperty.Register("NewHeight", typeof(int), typeof(ResizeDocumentPopup), new PropertyMetadata(0));

        public static readonly DependencyProperty NewWidthProperty =
            DependencyProperty.Register("NewWidth", typeof(int), typeof(ResizeDocumentPopup), new PropertyMetadata(0));

        public ResizeDocumentPopup()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            DataContext = this;
            Loaded += (_, _) => sizePicker.FocusWidthPicker();
        }

        public int NewHeight
        {
            get => (int)GetValue(NewHeightProperty);
            set => SetValue(NewHeightProperty, value);
        }


        public int NewWidth
        {
            get => (int)GetValue(NewWidthProperty);
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
