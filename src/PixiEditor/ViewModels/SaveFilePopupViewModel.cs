using Microsoft.Win32;
using PixiEditor.Helpers;
using PixiEditor.Models.Enums;
using PixiEditor.Models.IO;
using System.IO;
using System.Windows;

namespace PixiEditor.ViewModels;

internal class SaveFilePopupViewModel : ViewModelBase
{
    private string _filePath;
    private FileType _chosenFormat;

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

    public FileType ChosenFormat 
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
                
    /// <summary>
    ///     Command that handles Path choosing to save file
    /// </summary>
    private string ChoosePath()
    {
        SaveFileDialog path = new SaveFileDialog
        {
            Title = "Export path",
            CheckPathExists = true,
            Filter = SupportedFilesHelper.BuildSaveFilter(false),
            FilterIndex = 0
        };
        if (path.ShowDialog() == true)
        {
            if (string.IsNullOrEmpty(path.FileName) == false)
            {
                ChosenFormat = SupportedFilesHelper.ParseImageFormat(Path.GetExtension(path.SafeFileName));
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
