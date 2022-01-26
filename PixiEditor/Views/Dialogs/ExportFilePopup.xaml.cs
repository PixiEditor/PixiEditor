using PixiEditor.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace PixiEditor.Views
{
    public partial class ExportFilePopup : Window
    {
        public static readonly DependencyProperty SaveHeightProperty =
            DependencyProperty.Register("SaveHeight", typeof(int), typeof(ExportFilePopup), new PropertyMetadata(32));


        public static readonly DependencyProperty SaveWidthProperty =
            DependencyProperty.Register("SaveWidth", typeof(int), typeof(ExportFilePopup), new PropertyMetadata(32));

        private readonly SaveFilePopupViewModel dataContext = new SaveFilePopupViewModel();

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        public ExportFilePopup()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            DataContext = dataContext;
            Loaded += (_, _) => sizePicker.FocusWidthPicker();
        }


        public int SaveWidth
        {
            get => (int)GetValue(SaveWidthProperty);
            set => SetValue(SaveWidthProperty, value);
        }


        public int SaveHeight
        {
            get => (int)GetValue(SaveHeightProperty);
            set => SetValue(SaveHeightProperty, value);
        }

        public string SavePath
        {
            get => dataContext.FilePath;
            set => dataContext.FilePath = value;
        }
    }
}
