using Microsoft.Win32;
using PixiEditor.Helpers;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace PixiEditor.ViewModels
{
    internal class SaveFilePopupViewModel : ViewModelBase
    {
        ImageFormat[] _formats = new[] { ImageFormat.Png, ImageFormat.Jpeg, ImageFormat.Bmp, ImageFormat.Gif, ImageFormat.Tiff };
        private string _filePath;
        private ImageFormat _chosenFormat;

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
                    RaisePropertyChanged(nameof(FilePath));
                }
            }
        }

        public ImageFormat ChosenFormat 
        { 
            get => _chosenFormat;
            set
            {
                if (_chosenFormat != value)
                {
                    _chosenFormat = value;
                    RaisePropertyChanged(nameof(ChosenFormat));
                }
            }
        }

        string GetFormattedString(ImageFormat imageFormat)
        {
            var formatLower = imageFormat.ToString().ToLower();
            return $"{imageFormat} Image (.{formatLower}) | *.{formatLower}";
        }

        string BuildFilter()
        {
            var filter = string.Join("|", _formats.Select(i => GetFormattedString(i)));
            return filter;
        }

        ImageFormat ParseImageFormat(string fileExtension)
        {
            fileExtension = fileExtension.Replace(".", "");
            return (ImageFormat)typeof(ImageFormat)
                    .GetProperty(fileExtension, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase)
                    .GetValue(null);
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
                DefaultExt = "." + _formats.First().ToString().ToLower(),
                Filter = BuildFilter()
            };
            if (path.ShowDialog() == true)
            {
                if (string.IsNullOrEmpty(path.FileName) == false)
                {
                    ChosenFormat = ParseImageFormat(Path.GetExtension(path.SafeFileName));
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
