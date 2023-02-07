using System.Windows;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Enums;
using PixiEditor.Views;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.Models.Dialogs;

internal class ExportFileDialog : CustomDialog
{
    FileType _chosenFormat;

    private int fileHeight;

    private string filePath;

    private int fileWidth;

    public ExportFileDialog(VecI size)
    {
        FileWidth = size.X;
        FileHeight = size.Y;
    }

    public int FileWidth
    {
        get => fileWidth;
        set
        {
            if (fileWidth != value)
            {
                fileWidth = value;
                RaisePropertyChanged("Width");
            }
        }
    }

    public int FileHeight
    {
        get => fileHeight;
        set
        {
            if (fileHeight != value)
            {
                fileHeight = value;
                RaisePropertyChanged("FileHeight");
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

    public override bool ShowDialog()
    {
        ExportFilePopup popup = new ExportFilePopup(FileWidth, FileHeight);
        popup.ShowDialog();

        if (popup.DialogResult == true)
        {
            FileWidth = popup.SaveWidth;
            FileHeight = popup.SaveHeight;
            FilePath = popup.SavePath;

            ChosenFormat = popup.SaveFormat;
        }

        return (bool)popup.DialogResult;
    }
}
