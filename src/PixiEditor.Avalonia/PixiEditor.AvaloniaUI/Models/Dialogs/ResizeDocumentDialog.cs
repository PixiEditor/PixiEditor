using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using PixiEditor.AvaloniaUI.Helpers.Extensions;
using PixiEditor.AvaloniaUI.Views.Windows;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.AvaloniaUI.Models.Dialogs;

internal class ResizeDocumentDialog : CustomDialog
{
    private int height;
    private int width;

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
            NewSelectedUnit = SizeUnit.Pixel
        };

        var result = await popup.ShowDialog<bool>(OwnerWindow);
        if (result)
        {
            Width = popup.NewAbsoluteWidth;
            Height = popup.NewAbsoluteHeight;
            /*if (popup is ResizeCanvasPopup resizeCanvas) TODO: Implement
            {
                ResizeAnchor = resizeCanvas.SelectedAnchorPoint;
            }*/
        }

        return false;
    }

    private async Task<bool> ShowResizeDocumentCanvas()
    {
        //TODO: Implement
        //return await ShowDialog<ResizeDocumentPopup>();
        return false;
    }

    private async Task<bool> ShowResizeCanvasDialog()
    {
        //TODO: Implement
        //return await ShowDialog<ResizeCanvasPopup>();
        return false;
    }
}
