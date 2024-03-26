using System.Threading.Tasks;
using Avalonia.Controls;
using PixiEditor.AvaloniaUI.Models.Dialogs;
using PixiEditor.AvaloniaUI.Models.Files;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.Views.Dialogs;

internal class ExportFileDialog : CustomDialog
{
    FileType _chosenFormat;

    private int fileHeight;

    private string filePath;

    private int fileWidth;

    private string suggestedName;

    public ExportFileDialog(Window owner, VecI size) : base(owner)
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
                this.SetProperty(ref fileWidth, value);
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
                this.SetProperty(ref fileHeight, value);
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
                this.SetProperty(ref filePath, value);
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
                this.SetProperty(ref _chosenFormat, value);
            }
        }
    }

    public string SuggestedName
    {
        get => suggestedName;
        set
        {
            if (suggestedName != value)
            {
                this.SetProperty(ref suggestedName, value);
            }
        }
    }
    public override async Task<bool> ShowDialog()
    {
        ExportFilePopup popup = new ExportFilePopup(FileWidth, FileHeight) { SuggestedName = SuggestedName };
        bool result = await popup.ShowDialog<bool>(OwnerWindow);

        if (result)
        {
            FileWidth = popup.SaveWidth;
            FileHeight = popup.SaveHeight;
            FilePath = popup.SavePath;
            ChosenFormat = popup.SaveFormat;
        }

        return result;
    }
}
