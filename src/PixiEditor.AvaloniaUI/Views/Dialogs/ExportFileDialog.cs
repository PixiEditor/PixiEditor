using System.Threading.Tasks;
using Avalonia.Controls;
using PixiEditor.AnimationRenderer.FFmpeg;
using PixiEditor.AvaloniaUI.Models.Dialogs;
using PixiEditor.AvaloniaUI.Models.Files;
using PixiEditor.AvaloniaUI.Models.IO;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.AvaloniaUI.Views.Dialogs;

internal class ExportFileDialog : CustomDialog
{
    IoFileType _chosenFormat;

    private int fileHeight;

    private string filePath;

    private int fileWidth;

    private string suggestedName;
    
    private DocumentViewModel document;
    
    public ExportConfig ExportConfig { get; set; } = new ExportConfig();

    public ExportFileDialog(Window owner, VecI size, DocumentViewModel doc) : base(owner)
    {
        FileWidth = size.X;
        FileHeight = size.Y;
        document = doc;
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

    public IoFileType ChosenFormat
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
        ExportFilePopup popup = new ExportFilePopup(FileWidth, FileHeight, document) { SuggestedName = SuggestedName };
        bool result = await popup.ShowDialog<bool>(OwnerWindow);

        if (result)
        {
            FileWidth = popup.SaveWidth;
            FileHeight = popup.SaveHeight;
            FilePath = popup.SavePath;
            ChosenFormat = popup.SaveFormat;
            
            ExportConfig.ExportSize = new VecI(FileWidth, FileHeight);
            ExportConfig.AnimationRenderer = ChosenFormat is VideoFileType ? new FFMpegRenderer()
            {
                Size = new VecI(FileWidth, FileHeight),
                OutputFormat = ChosenFormat.PrimaryExtension.Replace(".", ""),
                FrameRate = document.AnimationDataViewModel.FrameRateBindable
            }
            : null;
            ExportConfig.ExportAsSpriteSheet = popup.IsSpriteSheetExport;
            ExportConfig.SpriteSheetColumns = popup.SpriteSheetColumns;
            ExportConfig.SpriteSheetRows = popup.SpriteSheetRows;
        }

        return result;
    }
}
