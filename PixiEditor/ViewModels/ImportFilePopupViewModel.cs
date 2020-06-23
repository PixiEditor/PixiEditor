using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using PixiEditor.Helpers;

namespace PixiEditor.ViewModels
{
    internal class ImportFilePopupViewModel : ViewModelBase
    {
        private string _filePath;


        private int _importHeight = 16;


        private int _importWidth = 16;


        private string _pathButtonBorder = "#f08080";


        private bool _pathIsCorrect;

        public ImportFilePopupViewModel()
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
                    CheckForPath(value);
                    RaisePropertyChanged("FilePath");
                }
            }
        }

        public int ImportWidth
        {
            get => _importWidth;
            set
            {
                if (_importWidth != value)
                {
                    _importWidth = value;
                    RaisePropertyChanged("ImportWidth");
                }
            }
        }

        public int ImportHeight
        {
            get => _importHeight;
            set
            {
                if (_importHeight != value)
                {
                    _importHeight = value;
                    RaisePropertyChanged("ImportHeight");
                }
            }
        }

        /// <summary>
        ///     Command that handles Path choosing to save file
        /// </summary>
        /// <param name="parameter"></param>
        private void ChoosePath(object parameter)
        {
            OpenFileDialog path = new OpenFileDialog
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
            if (File.Exists(path) && (path.EndsWith(".png") || path.EndsWith(".jpeg") || path.EndsWith(".jpg")))
            {
                PathButtonBorder = "#b8f080";
                PathIsCorrect = true;
                _filePath = path;
                BitmapImage bitmap = new BitmapImage(new Uri(path));
                ImportHeight = bitmap.PixelHeight;
                ImportWidth = bitmap.PixelWidth;
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