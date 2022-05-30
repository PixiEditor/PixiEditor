using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using ChunkyImageLib.DataHolders;
using PixiEditor.Zoombox;
using PixiEditorPrototype.Models;
using SkiaSharp;

namespace PixiEditorPrototype.ViewModels;

internal class ViewModelMain : INotifyPropertyChanged
{
    public DocumentViewModel? ActiveDocument => GetDocumentByGuid(activeDocumentGuid);

    public RelayCommand? MouseDownCommand { get; }
    public RelayCommand? MouseMoveCommand { get; }
    public RelayCommand? MouseUpCommand { get; }
    public RelayCommand? ChangeActiveToolCommand { get; }

    public Color SelectedColor { get; set; } = Colors.Black;

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool NormalZoombox
    {
        set
        {
            if (!value)
                return;
            ZoomboxMode = ZoomboxMode.Normal;
            PropertyChanged?.Invoke(this, new(nameof(ZoomboxMode)));
        }
    }

    public bool MoveZoombox
    {
        set
        {
            if (!value)
                return;
            ZoomboxMode = ZoomboxMode.Move;
            PropertyChanged?.Invoke(this, new(nameof(ZoomboxMode)));
        }
    }

    public bool RotateZoombox
    {
        set
        {
            if (!value)
                return;
            ZoomboxMode = ZoomboxMode.Rotate;
            PropertyChanged?.Invoke(this, new(nameof(ZoomboxMode)));
        }
    }

    public ZoomboxMode ZoomboxMode { get; set; }

    private Dictionary<Guid, DocumentViewModel> documents = new();
    private Guid activeDocumentGuid;

    private int mouseDownCanvasX = 0;
    private int mouseDownCanvasY = 0;

    private bool mouseHasMoved = false;
    private bool mouseIsDown = false;

    private Tool activeTool = Tool.Rectangle;
    private Tool toolOnMouseDown = Tool.Rectangle;

    public ViewModelMain()
    {
        MouseDownCommand = new RelayCommand(MouseDown);
        MouseMoveCommand = new RelayCommand(MouseMove);
        MouseUpCommand = new RelayCommand(MouseUp);
        ChangeActiveToolCommand = new RelayCommand(ChangeActiveTool);

        var doc = new DocumentViewModel(this);
        documents[doc.GuidValue] = doc;
        activeDocumentGuid = doc.GuidValue;
    }

    public DocumentViewModel? GetDocumentByGuid(Guid guid)
    {
        return documents.TryGetValue(guid, out DocumentViewModel? value) ? value : null;
    }

    private void MouseDown(object? param)
    {
        if (ActiveDocument is null || ZoomboxMode != ZoomboxMode.Normal || ActiveDocument.TransformViewModel.TransformActive)
            return;
        if (mouseIsDown)
        {
            mouseIsDown = false;
            ProcessToolMouseUp();
            mouseHasMoved = false;
        }

        mouseIsDown = true;
        var args = (MouseButtonEventArgs)(param!);
        var source = (System.Windows.Controls.Image)args.Source;
        var pos = args.GetPosition(source);
        mouseDownCanvasX = (int)(pos.X / source.Width * ActiveDocument.Bitmaps[ChunkResolution.Full].PixelWidth);
        mouseDownCanvasY = (int)(pos.Y / source.Height * ActiveDocument.Bitmaps[ChunkResolution.Full].PixelHeight);
        toolOnMouseDown = activeTool;
        ProcessToolMouseDown(mouseDownCanvasX, mouseDownCanvasY);
    }

    private void ProcessToolMouseDown(int canvasX, int canvasY)
    {
        if (toolOnMouseDown is Tool.FloodFill)
        {
            ActiveDocument!.FloodFill(new VecI(canvasX, canvasY), new SKColor(SelectedColor.R, SelectedColor.G, SelectedColor.B, SelectedColor.A));
        }
    }

    private void MouseMove(object? param)
    {
        if (!mouseIsDown)
            return;
        mouseHasMoved = true;
        var args = (MouseEventArgs)(param!);
        var source = (System.Windows.Controls.Image)args.Source;
        var pos = args.GetPosition(source);
        int curX = (int)(pos.X / source.Width * ActiveDocument!.Bitmaps[ChunkResolution.Full].PixelWidth);
        int curY = (int)(pos.Y / source.Height * ActiveDocument.Bitmaps[ChunkResolution.Full].PixelHeight);

        ProcessToolMouseMove(curX, curY);
    }

    private void ProcessToolMouseMove(int canvasX, int canvasY)
    {
        if (toolOnMouseDown == Tool.Rectangle)
        {
            int width = canvasX - mouseDownCanvasX;
            int height = canvasY - mouseDownCanvasY;
            ActiveDocument!.StartUpdateRectangle(new ShapeData(
                        new(mouseDownCanvasX + width / 2.0, mouseDownCanvasY + height / 2.0),
                        new(width, height),
                        0,
                        90,
                        new SKColor(SelectedColor.R, SelectedColor.G, SelectedColor.B, SelectedColor.A),
                        SKColors.Transparent));
        }
        else if (toolOnMouseDown == Tool.Select)
        {
            ActiveDocument!.StartUpdateSelection(
                new(mouseDownCanvasX, mouseDownCanvasY),
                new(canvasX - mouseDownCanvasX, canvasY - mouseDownCanvasY));
        }
        else if (toolOnMouseDown == Tool.ShiftLayer)
        {
            ActiveDocument!.StartUpdateShiftLayer(new(canvasX - mouseDownCanvasX, canvasY - mouseDownCanvasY));
        }
    }

    private void MouseUp(object? param)
    {
        if (!mouseIsDown)
            return;
        mouseIsDown = false;
        ProcessToolMouseUp();
        mouseHasMoved = false;
    }

    private void ProcessToolMouseUp()
    {
        if (mouseHasMoved)
        {
            switch (toolOnMouseDown)
            {
                case Tool.Rectangle:
                    ActiveDocument!.EndRectangleDrawing();
                    break;
                case Tool.Select:
                    ActiveDocument!.EndSelection();
                    break;
                case Tool.ShiftLayer:
                    ActiveDocument!.EndShiftLayer();
                    break;
            }
        }

    }

    private void ChangeActiveTool(object? param)
    {
        if (param is null)
            return;
        activeTool = (Tool)param;
    }
}
