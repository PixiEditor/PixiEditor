using System.Windows;
using Microsoft.Win32;
using PixiEditor.Helpers;

namespace PixiEditor.ViewModels
{
    internal class SaveFilePopupViewModel : ViewModelBase
    {
        private string filePath;


        private string pathButtonBorder = "#f08080";


        private bool pathIsCorrect;

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
            get => pathButtonBorder;
            set
            {
                if (pathButtonBorder != value)
                {
                    pathButtonBorder = value;
                    RaisePropertyChanged("PathButtonBorder");
                }
            }
        }

        public bool PathIsCorrect
        {
            get => pathIsCorrect;
            set
            {
                if (pathIsCorrect != value)
                {
                    pathIsCorrect = value;
                    RaisePropertyChanged("PathIsCorrect");
                }
            }
        }

        public string FilePath
        {
            get => filePath;
            set
            {
                if (filePath != value)
                {
                    filePath = value;
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
            var path = new SaveFileDialog
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
            ((Window) parameter).DialogResult = false;
            CloseButton(parameter);
        }

        private void MoveWindow(object parameter)
        {
            DragMove(parameter);
        }

        private void OkButton(object parameter)
        {
            ((Window) parameter).DialogResult = true;
            CloseButton(parameter);
        }

        private bool CanClickOk(object property)
        {
            return PathIsCorrect;
        }
    }
}