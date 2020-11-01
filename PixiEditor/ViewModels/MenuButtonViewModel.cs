using System.Windows;
using PixiEditor.Helpers;

namespace PixiEditor.ViewModels
{
    internal class MenuButtonViewModel : ViewModelBase
    {
        private Visibility listViewVisibility;

        public MenuButtonViewModel()
        {
            OpenListViewCommand = new RelayCommand(OpenListView);
            CloseListViewCommand = new RelayCommand(CloseListView);
            ListViewVisibility = Visibility.Hidden;
        }

        public RelayCommand OpenListViewCommand { get; set; }

        public RelayCommand CloseListViewCommand { get; set; }

        public Visibility ListViewVisibility
        {
            get => listViewVisibility;
            set
            {
                listViewVisibility = value;
                RaisePropertyChanged("ListViewVisibility");
            }
        }

        private void OpenListView(object parameter)
        {
            ListViewVisibility = ListViewVisibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;
        }

        private void CloseListView(object parameter)
        {
            ListViewVisibility = Visibility.Hidden;
        }
    }
}