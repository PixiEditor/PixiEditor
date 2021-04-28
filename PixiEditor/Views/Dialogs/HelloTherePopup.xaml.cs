using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;
using PixiEditor.ViewModels.SubViewModels.Main;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace PixiEditor.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for HelloTherePopup.xaml.
    /// </summary>
    public partial class HelloTherePopup : Window
    {
        public RecentlyOpenedCollection RecentlyOpened { get => FileViewModel.RecentlyOpened; }

        public static readonly DependencyProperty FileViewModelProperty =
            DependencyProperty.Register(nameof(FileViewModel), typeof(FileViewModel), typeof(HelloTherePopup));

        public static readonly DependencyProperty RecentlyOpenedEmptyProperty =
            DependencyProperty.Register(nameof(RecentlyOpenedEmpty), typeof(bool), typeof(HelloTherePopup));

        public static string VersionText => 
            $"v{AssemblyHelper.GetCurrentAssemblyVersion(x => $"{x.Major}.{x.Minor}" + (x.Revision != 0 ? $".{x.Revision}" : ""))}";

        public FileViewModel FileViewModel { get => (FileViewModel)GetValue(FileViewModelProperty); set => SetValue(FileViewModelProperty, value); }

        public bool RecentlyOpenedEmpty { get => (bool)GetValue(RecentlyOpenedEmptyProperty); set => SetValue(RecentlyOpenedEmptyProperty, value); }

        public RelayCommand OpenFileCommand { get; set; }

        public RelayCommand OpenNewFileCommand { get; set; }

        public RelayCommand OpenRecentCommand { get; set; }

        public RelayCommand OpenHyperlinkCommand { get => FileViewModel.Owner.MiscSubViewModel.OpenHyperlinkCommand; }

        private bool isClosing;

        public HelloTherePopup(FileViewModel fileViewModel)
        {
            DataContext = this;
            FileViewModel = fileViewModel;

            OpenFileCommand = new RelayCommand(OpenFile);
            OpenNewFileCommand = new RelayCommand(OpenNewFile);
            OpenRecentCommand = new RelayCommand(OpenRecent);

            RecentlyOpenedEmpty = RecentlyOpened.Count == 0;
            RecentlyOpened.CollectionChanged += RecentlyOpened_CollectionChanged;

            Closing += (_, _) => { isClosing = true; };

            InitializeComponent();

            if (RecentlyOpenedEmpty)
            {
                Height = 500;
                Width = 520;
            }
            else if (RecentlyOpened.Count < 7)
            {
                Height = 676;
                Width = 545;
            }
        }

        private void RecentlyOpened_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RecentlyOpenedEmpty = FileViewModel.RecentlyOpened.Count == 0;
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void OpenFile(object parameter)
        {
            Close();
            FileViewModel.OpenAny();
        }

        private void OpenNewFile(object parameter)
        {
            Close();
            FileViewModel.OpenNewFilePopup(parameter);
        }

        private void OpenRecent(object parameter)
        {
            FileViewModel.OpenRecent(parameter);
            Close();
        }
    }
}
