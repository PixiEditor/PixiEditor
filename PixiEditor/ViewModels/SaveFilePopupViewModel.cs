using Microsoft.Win32;
using PixiEditor.Helpers;
using System.Windows;

namespace PixiEditor.ViewModels
{
    class SaveFilePopupViewModel : ViewModelBase
    {
        public RelayCommand CloseButtonCommand { get; set; }
        public RelayCommand DragMoveCommand { get; set; }
        public RelayCommand ChoosePathCommand { get; set; }
        public RelayCommand OkCommand { get; set; }


        private string _pathButtonBorder = "#f08080";

        public string PathButtonBorder
        {
            get { return _pathButtonBorder; }
            set { if (_pathButtonBorder != value) { _pathButtonBorder = value; RaisePropertyChanged("PathButtonBorder"); } }
        }


        private bool _pathIsCorrect;

        public bool PathIsCorrect
        {
            get { return _pathIsCorrect; }
            set { if (_pathIsCorrect != value) { _pathIsCorrect = value; RaisePropertyChanged("PathIsCorrect"); } }
        }


        private string _filePath;

        public string FilePath
        {
            get { return _filePath; }
            set { if (_filePath != value) { _filePath = value; RaisePropertyChanged("FilePath"); } }
        }

        public SaveFilePopupViewModel()
        {
            CloseButtonCommand = new RelayCommand(CloseWindow);
            DragMoveCommand = new RelayCommand(MoveWindow);
            ChoosePathCommand = new RelayCommand(ChoosePath);
            OkCommand = new RelayCommand(OkButton, CanClickOk);
        }

        /// <summary>
        /// Command that handles Path choosing to save file
        /// </summary>
        /// <param name="parameter"></param>
        private void ChoosePath(object parameter)
        {
            SaveFileDialog path = new SaveFileDialog()
            {
                Title = "Export path",
                CheckPathExists = true,
                DefaultExt = "PNG Image (.png)|*.png",
                Filter = "PNG Image (.png)|*.png"
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
            base.CloseButton(parameter);
        }

        private void MoveWindow(object parameter)
        {
            base.DragMove(parameter);
        }

        private void OkButton(object parameter)
        {
            ((Window)parameter).DialogResult = true;
            base.CloseButton(parameter);
        }

        private bool CanClickOk(object property)
        {
            return PathIsCorrect == true;
        }
    }
}
