using System.Windows;
using PixiEditor.Helpers;

namespace PixiEditor.ViewModels
{
    internal class NewFileMenuViewModel : ViewModelBase
    {
        public NewFileMenuViewModel()
        {
            OkCommand = new RelayCommand(OkButton);
            CloseCommand = new RelayCommand(CloseWindow);
            DragMoveCommand = new RelayCommand(MoveWindow);
        }

        public RelayCommand OkCommand { get; set; }

        public RelayCommand CloseCommand { get; set; }

        public RelayCommand DragMoveCommand { get; set; }

        private void OkButton(object parameter)
        {
            ((Window)parameter).DialogResult = true;
            ((Window)parameter).Close();
        }

        private void CloseWindow(object parameter)
        {
            ((Window)parameter).DialogResult = false;
            CloseButton(parameter);
        }

        private void MoveWindow(object parameter)
        {
            DragMove(parameter);
        }
    }
}