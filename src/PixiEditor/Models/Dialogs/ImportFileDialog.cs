using PixiEditor.Views;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.Models.Dialogs;

internal class ImportFileDialog : CustomDialog
{
    private int fileHeight;

    private string filePath;
    private int fileWidth;

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

    public override bool ShowDialog()
    {
        ImportFilePopup popup = new ImportFilePopup
        {
            FilePath = FilePath
        };

        if (FileWidth != 0) popup.ImportWidth = FileWidth;
        if (FileHeight != 0) popup.ImportHeight = FileHeight;
        
        popup.ShowDialog();
        if (popup.DialogResult == true)
        {
            FileHeight = popup.ImportHeight;
            FileWidth = popup.ImportWidth;
            FilePath = popup.FilePath;
        }

        return (bool)popup.DialogResult;
    }
}
