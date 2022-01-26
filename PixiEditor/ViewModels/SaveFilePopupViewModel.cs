using Microsoft.Win32;
using PixiEditor.Helpers;
using System.Windows;

namespace PixiEditor.ViewModels
{
    internal class SaveFilePopupViewModel : ViewModelBase
    {
        private string _filePath;

        public SaveFilePopupViewModel()
        {
            CloseButtonCommand = new RelayCommand(CloseWindow);
            DragMoveCommand = new RelayCommand(MoveWindow);
            OkCommand = new RelayCommand(OkButton);
        }

        public RelayCommand CloseButtonCommand { get; set; }
        public RelayCommand DragMoveCommand { get; set; }
        public RelayCommand OkCommand { get; set; }

        public string FilePath
        {
            get => _filePath;
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    RaisePropertyChanged("FilePath");
                }
            }
        }

        /// <summary>
        ///     Command that handles Path choosing to save file
        /// </summary>
        private string ChoosePath()
        {
            SaveFileDialog path = new SaveFileDialog
            {
                Title = "Export path",
                CheckPathExists = true,
                DefaultExt = "PNG Image (.png) | *.png",
                Filter = "PNG Image (.png) | *.png"
            };
            if (path.ShowDialog() == true)
            {
                if (string.IsNullOrEmpty(path.FileName) == false)
                {
                    return path.FileName;
                }
            }
            return null;
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

        private void OkButton(object parameter)
        {
            string path = ChoosePath();
            if (path == null)
                return;
            FilePath = path;
            ((Window)parameter).DialogResult = true;
            CloseButton(parameter);
        }
    }
}
