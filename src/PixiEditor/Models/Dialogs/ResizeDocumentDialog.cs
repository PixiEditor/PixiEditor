using Avalonia.Controls;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.UI.Common.Converters;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.Models.Dialogs;

internal class ResizeDocumentDialog : CustomDialog
{
    private int height;
    private int width;

    private string sizeUnit = "px";
    private MeasurementConverterData? unitConverter;

    public ResizeDocumentDialog(int currentWidth, int currentHeight, Window owner, bool openResizeCanvas = false) : base(owner)
    {
        Width = currentWidth;
        Height = currentHeight;
        OpenResizeCanvas = openResizeCanvas;
    }

    public bool OpenResizeCanvas { get; set; }

    public ResizeAnchor ResizeAnchor { get; set; }

    public int Width
    {
        get => width;
        set
        {
            if (width != value)
            {
                width = value;
                OnPropertyChanged(nameof(Width));
            }
        }
    }

    public int Height
    {
        get => height;
        set
        {
            if (height != value)
            {
                height = value;
                OnPropertyChanged(nameof(Height));
            }
        }
    }

    public string Unit
    {
        get => sizeUnit;
        set
        {
            SetProperty(ref sizeUnit, value);
        }
    }

    public MeasurementConverterData? MeasurementConverterData
    {
        get => unitConverter;
        set
        {
            SetProperty(ref unitConverter, value);
        }
    }

    public ResamplingMethod SelectedSamplingMethod { get; set; } = ResamplingMethod.NearestNeighbor;

    public override async Task<bool> ShowDialog()
    {
        return OpenResizeCanvas ? await ShowResizeCanvasDialog() : await ShowResizeDocumentCanvas();
    }

    async Task<bool> ShowDialog<T>()
        where T : ResizeablePopup, new()
    {
        T popup = new T()
        {
            NewAbsoluteHeight = Height,
            NewAbsoluteWidth = Width,
            NewPercentageSize = 100,
            NewSelectedUnit = SizeUnit.Pixel,
            Unit = Unit,
            MeasurementConverter =  MeasurementConverterData,
        };

        var result = await popup.ShowDialog<bool>(OwnerWindow);
        if (result)
        {
            Width = popup.NewAbsoluteWidth;
            Height = popup.NewAbsoluteHeight;
            if (popup is ResizeCanvasPopup resizeCanvas)
            {
                ResizeAnchor = resizeCanvas.SelectedAnchorPoint;
            }
            if(popup is ResizeDocumentPopup resizeDocument)
            {
                SelectedSamplingMethod = resizeDocument.Sampling;
            }

            return true;
        }

        return false;
    }

    private async Task<bool> ShowResizeDocumentCanvas()
    {
        return await ShowDialog<ResizeDocumentPopup>();
    }

    private async Task<bool> ShowResizeCanvasDialog()
    {
        return await ShowDialog<ResizeCanvasPopup>();
    }
}
