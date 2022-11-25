using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Models.Enums;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.Models.Dialogs;

internal class ResizeDocumentDialog : CustomDialog
{
    private int height;
    private int width;

    public ResizeDocumentDialog(int currentWidth, int currentHeight, bool openResizeCanvas = false)
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
                RaisePropertyChanged(nameof(Width));
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
                RaisePropertyChanged(nameof(Height));
            }
        }
    }

    public override bool ShowDialog()
    {
        return OpenResizeCanvas ? ShowResizeCanvasDialog() : ShowResizeDocumentCanvas();
    }

    bool ShowDialog<T>()
        where T : ResizeablePopup, new()
    {
        T popup = new T()
        {
            NewAbsoluteHeight = Height,
            NewAbsoluteWidth = Width,
            NewPercentageSize = 100,
            NewSelectedUnit = SizeUnit.Pixel
        };

        popup.ShowDialog();
        if (popup.DialogResult == true)
        {
            Width = popup.NewAbsoluteWidth;
            Height = popup.NewAbsoluteHeight;
            if (popup is ResizeCanvasPopup resizeCanvas)
            {
                ResizeAnchor = resizeCanvas.SelectedAnchorPoint;
            }
        }

        return (bool)popup.DialogResult;
    }

    private bool ShowResizeDocumentCanvas()
    {
        return ShowDialog<ResizeDocumentPopup>();
    }

    private bool ShowResizeCanvasDialog()
    {
        return ShowDialog<ResizeCanvasPopup>();
    }
}
