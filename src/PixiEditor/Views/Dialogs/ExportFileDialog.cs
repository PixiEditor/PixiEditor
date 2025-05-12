using System.Threading.Tasks;
using Avalonia.Controls;
using PixiEditor.AnimationRenderer.FFmpeg;
using Drawie.Backend.Core.Numerics;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Files;
using PixiEditor.Models.IO;
using Drawie.Numerics;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Views.Dialogs;

internal class ExportFileDialog : CustomDialog
{
    IoFileType _chosenFormat;

    private int fileHeight;

    private string filePath;

    private int fileWidth;

    private int offsetX;
    private int offsetY;

    private string suggestedName;
    
    private DocumentViewModel document;
    
    public ExportConfig ExportConfig { get; set; } = new ExportConfig(VecI.Zero);

    public ExportFileDialog(Window owner, DocumentViewModel doc) : base(owner)
    {
        RectI zone = doc.GetDefaultRenderZone();
        FileWidth = zone.Width;
        FileHeight = zone.Height;

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

    public int OffsetX
    {
        get => offsetX;
        set
        {
            if (offsetX != value)
            {
                this.SetProperty(ref offsetX, value);
            }
        }
    }

    public int OffsetY
    {
        get => offsetY;
        set
        {
            if (offsetY != value)
            {
                this.SetProperty(ref offsetY, value);
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
            
            if (ChosenFormat is SvgFileType)
            {
                ExportConfig.VectorExportConfig = new VectorExportConfig()
                {
                    UseNearestNeighborForImageUpscaling =
                        ExportConfig.ExportSize.X < 512 || ExportConfig.ExportSize.Y < 512
                };
            }
        }

        return result;
    }
}
