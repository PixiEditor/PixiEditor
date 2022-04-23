using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using ChunkyImageLib.DataHolders;
using PixiEditor.Zoombox;
using PixiEditorPrototype.Models;
using PixiEditorPrototype.Views;
using SkiaSharp;

namespace PixiEditorPrototype.ViewModels;

internal class ViewModelMain : INotifyPropertyChanged
{
    public IMainView? View { get; set; }

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

    public ViewportViewModel MainViewport { get; }

    private Dictionary<Guid, DocumentViewModel> documents = new();
    private Guid activeDocumentGuid;

    private bool mouseIsDown = false;
    private int mouseDownCanvasX = 0;
    private int mouseDownCanvasY = 0;

    private bool startedDrawingRect = false;
    private bool startedSelectingRect = false;

    private Tool activeTool = Tool.Rectangle;

    public ViewModelMain()
    {
        MouseDownCommand = new RelayCommand(MouseDown);
        MouseMoveCommand = new RelayCommand(MouseMove);
        MouseUpCommand = new RelayCommand(MouseUp);
        ChangeActiveToolCommand = new RelayCommand(ChangeActiveTool);

        var doc = new DocumentViewModel(this);
        documents[doc.GuidValue] = doc;
        activeDocumentGuid = doc.GuidValue;

        MainViewport = new(this, activeDocumentGuid);
        doc.RefreshViewport(MainViewport.GuidValue);
    }

    public ViewportLocation? GetViewport(Guid viewportGuid)
    {
        if (MainViewport.GuidValue != viewportGuid)
            return null;
        return new ViewportLocation(MainViewport.Angle, MainViewport.Center, MainViewport.RealDimensions, MainViewport.Dimensions, Guid.Empty);
    }

    public DocumentViewModel? GetDocumentByGuid(Guid guid)
    {
        return documents.TryGetValue(guid, out DocumentViewModel? value) ? value : null;
    }

    public void UpdateViewportResolution(Guid viewportGuid, ChunkResolution resolution)
    {
        if (viewportGuid != MainViewport.GuidValue)
            return;
        MainViewport.Resolution = resolution;
    }

    private void MouseDown(object? param)
    {
        if (ActiveDocument is null || ZoomboxMode != ZoomboxMode.Normal)
            return;
        mouseIsDown = true;
        var args = (MouseButtonEventArgs)(param!);
        var source = (System.Windows.Controls.Image)args.Source;
        var pos = args.GetPosition(source);
        mouseDownCanvasX = (int)(pos.X / source.Width * ActiveDocument.Bitmaps[ChunkResolution.Full].PixelWidth);
        mouseDownCanvasY = (int)(pos.Y / source.Height * ActiveDocument.Bitmaps[ChunkResolution.Full].PixelHeight);
    }

    private void MouseMove(object? param)
    {
        if (ActiveDocument is null || !mouseIsDown || ZoomboxMode != ZoomboxMode.Normal)
            return;
        var args = (MouseEventArgs)(param!);
        var source = (System.Windows.Controls.Image)args.Source;
        var pos = args.GetPosition(source);
        int curX = (int)(pos.X / source.Width * ActiveDocument.Bitmaps[ChunkResolution.Full].PixelWidth);
        int curY = (int)(pos.Y / source.Height * ActiveDocument.Bitmaps[ChunkResolution.Full].PixelHeight);

        ProcessToolMouseMove(curX, curY);
    }

    private void ProcessToolMouseMove(int canvasX, int canvasY)
    {
        if (activeTool == Tool.Rectangle)
        {
            startedDrawingRect = true;
            ActiveDocument!.StartUpdateRectangle(new ShapeData(
                        new(mouseDownCanvasX, mouseDownCanvasY),
                        new(canvasX - mouseDownCanvasX, canvasY - mouseDownCanvasY),
                        90,
                        new SKColor(SelectedColor.R, SelectedColor.G, SelectedColor.B, SelectedColor.A),
                        SKColors.Transparent));
        }
        else if (activeTool == Tool.Select)
        {
            startedSelectingRect = true;
            ActiveDocument!.StartUpdateSelection(
                new(mouseDownCanvasX, mouseDownCanvasY),
                new(canvasX - mouseDownCanvasX, canvasY - mouseDownCanvasY));
        }
    }

    private void MouseUp(object? param)
    {
        if (ActiveDocument is null || !mouseIsDown || ZoomboxMode != ZoomboxMode.Normal)
            return;
        mouseIsDown = false;
        ProcessToolMouseUp();
    }

    private void ProcessToolMouseUp()
    {
        if (startedDrawingRect)
        {
            startedDrawingRect = false;
            ActiveDocument!.EndRectangle();
        }
        if (startedSelectingRect)
        {
            startedSelectingRect = false;
            ActiveDocument!.EndSelection();
        }
    }

    private void ChangeActiveTool(object? param)
    {
        if (param is null)
            return;
        activeTool = (Tool)param;
    }
}
