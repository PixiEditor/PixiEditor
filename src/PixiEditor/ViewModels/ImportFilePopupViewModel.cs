using PixiEditor.Exceptions;
using PixiEditor.Helpers;
using PixiEditor.Models.IO;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PixiEditor.ViewModels;

internal class ImportFilePopupViewModel : ViewModelBase
{
    private string filePath;

    private int importHeight = 16;

    private int importWidth = 16;
    public ImportFilePopupViewModel()
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
        get => filePath;
        set
        {
            if (filePath != value)
            {
                filePath = value;
                CheckForPath(value);
                RaisePropertyChanged(nameof(FilePath));
            }
        }
    }

    public int ImportWidth
    {
        get => importWidth;
        set
        {
            if (importWidth != value)
            {
                importWidth = value;
                RaisePropertyChanged(nameof(ImportWidth));
            }
        }
    }

    public int ImportHeight
    {
        get => importHeight;
        set
        {
            if (importHeight != value)
            {
                importHeight = value;
                RaisePropertyChanged(nameof(ImportHeight));
            }
        }
    }

    private void CheckForPath(string path)
    {
        if (SupportedFilesHelper.IsSupportedFile(path))
        {
            try
            {
                filePath = path;
                var bitmap = new BitmapImage(new Uri(path));
                ImportHeight = bitmap.PixelHeight;
                ImportWidth = bitmap.PixelWidth;
            }
            catch (Exception e) when (e is NotSupportedException or FileFormatException)
            {
                throw new CorruptedFileException("FAILED_TO_OPEN_FILE", e);
            }
            catch (COMException e)
            {
                throw new RecoverableException("INTERNAL_ERROR", e);
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
}
