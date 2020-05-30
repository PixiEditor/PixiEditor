using PixiEditor.Helpers;
using System.Windows;

namespace PixiEditor.ViewModels
{
    class NewFileMenuViewModel : ViewModelBase
    {
        public RelayCommand OkCommand { get; set; }
        public RelayCommand CloseCommand { get; set; }
        public RelayCommand DragMoveCommand { get; set; }

        public NewFileMenuViewModel()
        {
            OkCommand = new RelayCommand(OkButton);
            CloseCommand = new RelayCommand(CloseWindow);
            DragMoveCommand = new RelayCommand(MoveWindow);
        }

        private void OkButton(object parameter)
        {
            ((Window)parameter).DialogResult = true;
            ((Window)parameter).Close();
        }

        private void CloseWindow(object parameter)
        {
            ((Window)parameter).DialogResult = false;
            base.CloseButton(parameter);
        }

        private void MoveWindow(object parameter)
        {
            base.DragMove(parameter);
        }
    }
}
