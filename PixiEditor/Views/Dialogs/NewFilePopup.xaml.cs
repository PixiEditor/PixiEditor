using System.Windows;
using System.Windows.Input;

namespace PixiEditor.Views
{
    /// <summary>
    ///     Interaction logic for NewFilePopup.xaml.
    /// </summary>
    public partial class NewFilePopup : Window
    {
        // Using a DependencyProperty as the backing store for FileHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FileHeightProperty =
            DependencyProperty.Register("FileHeight", typeof(int), typeof(NewFilePopup), new PropertyMetadata(16));

        // Using a DependencyProperty as the backing store for FileWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FileWidthProperty =
            DependencyProperty.Register("FileWidth", typeof(int), typeof(NewFilePopup), new PropertyMetadata(16));

        public NewFilePopup()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
        }

        public int FileHeight
        {
            get => (int)GetValue(FileHeightProperty);
            set => SetValue(FileHeightProperty, value);
        }

        public int FileWidth
        {
            get => (int)GetValue(FileWidthProperty);
            set => SetValue(FileWidthProperty, value);
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }
    }
}