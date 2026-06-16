using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using PixiEditor.AnimationRenderer.FFmpeg;
using Drawie.Backend.Core.Numerics;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Files;
using PixiEditor.Models.IO;
using Drawie.Numerics;
using PixiEditor.AnimationRenderer.Core;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Views.Dialogs;

internal class ExportFileDialog : CustomDialog
{
    IoFileType _chosenFormat;

    private int fileHeight;

    private string filePath;

    private int fileWidth;

    private RenderOutputConfig exportOutput;

    private string suggestedName;

    private ObservableCollection<RenderOutputConfig> availableExportOutputs = new ObservableCollection<RenderOutputConfig>();
    
    private DocumentViewModel document;
    
    public ExportConfig ExportConfig { get; set; } = new ExportConfig(VecI.Zero);

    public ExportFileDialog(Window owner, DocumentViewModel doc) : base(owner)
    {
        AvailableExportOutputs = new ObservableCollection<RenderOutputConfig>(doc.GetAvailableExportOutputs().Select(x => new RenderOutputConfig(x.name, x.originalSize)));
        VecI size = doc.GetDefaultRenderSize(out string? renderOutputName);
        FileWidth = size.X;
        FileHeight = size.Y;
        ExportOutput = new RenderOutputConfig(renderOutputName, size);

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

    public ObservableCollection<RenderOutputConfig> AvailableExportOutputs
    {
        get => availableExportOutputs;
        set
        {
            if (availableExportOutputs != value)
            {
                this.SetProperty(ref availableExportOutputs, value);
            }
        }
    }

    public RenderOutputConfig ExportOutput
    {
        get => exportOutput;
        set
        {
            if (exportOutput != value)
            {
                this.SetProperty(ref exportOutput, value);
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
        ExportFilePopup popup = new ExportFilePopup(FileWidth, FileHeight, document)
        {
            SuggestedName = SuggestedName,
            AvailableExportOutputs = AvailableExportOutputs,
            ExportOutput = ExportOutput,
        };
        bool result = await popup.ShowDialog<bool>(OwnerWindow);

        if (result)
        {
            FileWidth = popup.SaveWidth;
            FileHeight = popup.SaveHeight;
            FilePath = popup.SavePath;
            ChosenFormat = popup.SaveFormat;
            ExportOutput = popup.ExportOutput;

            ExportConfig.ExportSize = new VecI(FileWidth, FileHeight);
            ExportConfig.ExportOutput = ExportOutput.Name;
            ExportConfig.ExportFramesToFolder = popup.FolderExport;
            ExportConfig.AnimationRenderer = new FFMpegRenderer()
            {
                Size = new VecI(FileWidth, FileHeight),
                OutputFormat = ChosenFormat.PrimaryExtension.Replace(".", ""),
                FrameRate = document.AnimationDataViewModel.FrameRateBindable,
                QualityPreset = (QualityPreset)popup.AnimationPresetIndex
            };
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

public record RenderOutputConfig
{
    public string Name { get; set; }
    public VecI OriginalSize { get; set; }

    public RenderOutputConfig(string name, VecI originalSize)
    {
        Name = name;
        OriginalSize = originalSize;
    }
}
