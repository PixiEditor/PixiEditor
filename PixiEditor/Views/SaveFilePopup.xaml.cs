using System.Windows;
using PixiEditor.ViewModels;

namespace PixiEditor.Views
{
    /// <summary>
    ///     Interaction logic for SaveFilePopup.xaml.
    /// </summary>
    public partial class SaveFilePopup : Window
    {
        // Using a DependencyProperty as the backing store for SaveHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SaveHeightProperty =
            DependencyProperty.Register("SaveHeight", typeof(int), typeof(SaveFilePopup), new PropertyMetadata(32));

        // Using a DependencyProperty as the backing store for SaveWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SaveWidthProperty =
            DependencyProperty.Register("SaveWidth", typeof(int), typeof(SaveFilePopup), new PropertyMetadata(32));

        private readonly SaveFilePopupViewModel dataContext = new SaveFilePopupViewModel();

        public SaveFilePopup()
        {
            InitializeComponent();
            DataContext = dataContext;
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