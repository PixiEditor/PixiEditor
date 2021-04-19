using Microsoft.Win32;
using PixiEditor.Helpers;
using System.Windows;

namespace PixiEditor.ViewModels
{
    internal class SaveFilePopupViewModel : ViewModelBase
    {
        private string _filePath;


        private string _pathButtonBorder = "#f08080";


        private bool _pathIsCorrect;

        public SaveFilePopupViewModel()
        {
            CloseButtonCommand = new RelayCommand(CloseWindow);
            DragMoveCommand = new RelayCommand(MoveWindow);
            ChoosePathCommand = new RelayCommand(ChoosePath);
            OkCommand = new RelayCommand(OkButton, CanClickOk);
        }

        public RelayCommand CloseButtonCommand { get; set; }
        public RelayCommand DragMoveCommand { get; set; }
        public RelayCommand ChoosePathCommand { get; set; }
        public RelayCommand OkCommand { get; set; }

        public string PathButtonBorder
        {
            get => _pathButtonBorder;
            set
            {
                if (_pathButtonBorder != value)
                {
                    _pathButtonBorder = value;
                    RaisePropertyChanged("PathButtonBorder");
                }
            }
        }

        public bool PathIsCorrect
        {
            get => _pathIsCorrect;
            set
            {
                if (_pathIsCorrect != value)
                {
                    _pathIsCorrect = value;
                    RaisePropertyChanged("PathIsCorrect");
                }
            }
        }

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
        /// <param name="parameter"></param>
        private void ChoosePath(object parameter)
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
                    PathButtonBorder = "#b8f080";
                    PathIsCorrect = true;
                    FilePath = path.FileName;
                }
                else
                {
                    PathButtonBorder = "#f08080";
                    PathIsCorrect = false;
                }
            }
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
            ((Window)parameter).DialogResult = true;
            CloseButton(parameter);
        }

        private bool CanClickOk(object property)
        {
            return PathIsCorrect;
        }
    }
}
