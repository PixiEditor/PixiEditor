using Microsoft.Win32;
using PixiEditor.Helpers;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PixiEditor.ViewModels
{
    class ImportFilePopupViewModel : ViewModelBase
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
            set { if (_filePath != value) 
                {
                    _filePath = value;
                    CheckForPath(value);
                    RaisePropertyChanged("FilePath"); 
                } 
            }
        }


        private int _importWidth = 16;

        public int ImportWidth
        {
            get { return _importWidth; }
            set { if (_importWidth != value) { _importWidth = value; RaisePropertyChanged("ImportWidth"); } }
        }


        private int _importHeight = 16;

        public int ImportHeight
        {
            get { return _importHeight; }
            set { if (_importHeight != value) { _importHeight = value; RaisePropertyChanged("ImportHeight"); } }
        }

        public ImportFilePopupViewModel()
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
            OpenFileDialog path = new OpenFileDialog()
            {
                Title = "Import path",
                CheckPathExists = true,
                Filter = "Image Files|*.png;*.jpeg;*.jpg"
            };
            if (path.ShowDialog() == true)
            {
                if (string.IsNullOrEmpty(path.FileName) == false)
                {
                    CheckForPath(path.FileName);
                }
                else
                {
                    PathButtonBorder = "#f08080";
                    PathIsCorrect = false;
                }
            }
        }

        private void CheckForPath(string path)
        {
            if(File.Exists(path) && (path.EndsWith(".png") || path.EndsWith(".jpeg") || path.EndsWith(".jpg")))
            {
                PathButtonBorder = "#b8f080";
                PathIsCorrect = true;
                _filePath = path;
                BitmapImage bitmap = new BitmapImage(new Uri(path));
                ImportHeight = (int)bitmap.Height;
                ImportWidth = (int)bitmap.Width;
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
