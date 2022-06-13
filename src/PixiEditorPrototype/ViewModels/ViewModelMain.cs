using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ChunkyImageLib.DataHolders;
using Microsoft.Win32;
using PixiEditor.Parser;
using PixiEditor.Zoombox;
using PixiEditorPrototype.Models;
using SkiaSharp;
using SelectionMode = PixiEditor.ChangeableDocument.Enums.SelectionMode;

namespace PixiEditorPrototype.ViewModels;

internal class ViewModelMain : INotifyPropertyChanged
{
    public DocumentViewModel? ActiveDocument => ActiveDocumentIndex >= 0 && ActiveDocumentIndex < Documents.Count ? Documents[ActiveDocumentIndex] : null;

    public RelayCommand MouseDownCommand { get; }
    public RelayCommand MouseMoveCommand { get; }
    public RelayCommand MouseUpCommand { get; }
    public RelayCommand ChangeActiveToolCommand { get; }
    public RelayCommand SetSelectionModeCommand { get; }
    public RelayCommand LoadDocumentCommand { get; }

    public Color SelectedColor { get; set; } = Colors.Black;
    public bool KeepOriginalImageOnTransform { get; set; } = false;
    public bool FillAllLayers { get; set; } = false;
    public float StrokeWidth { get; set; } = 1f;

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

    private int activeDocumentIndex;

    public int ActiveDocumentIndex
    {
        get => activeDocumentIndex;
        set
        {
            activeDocumentIndex = value;
            PropertyChanged?.Invoke(this, new(nameof(ActiveDocumentIndex)));
            PropertyChanged?.Invoke(this, new(nameof(ActiveDocument)));
        }
    }

    public ZoomboxMode ZoomboxMode { get; set; }

    public ObservableCollection<DocumentViewModel> Documents { get; } = new();


    private VecD mouseDownCanvasPos;

    private bool mouseHasMoved = false;
    private bool mouseIsDown = false;

    private Tool activeTool = Tool.Rectangle;
    private SelectionMode selectionMode = SelectionMode.New;

    private Tool toolOnMouseDown = Tool.Rectangle;

    public ViewModelMain()
    {
        MouseDownCommand = new RelayCommand(MouseDown);
        MouseMoveCommand = new RelayCommand(MouseMove);
        MouseUpCommand = new RelayCommand(MouseUp);
        ChangeActiveToolCommand = new RelayCommand(ChangeActiveTool);
        SetSelectionModeCommand = new RelayCommand(SetSelectionMode);
        LoadDocumentCommand = new RelayCommand(LoadDocument);

        Documents.Add(new DocumentViewModel(this, "New Artwork"));
    }

    private void SetSelectionMode(object? obj)
    {
        if (obj is not SelectionMode mode)
            return;
        selectionMode = mode;
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
        var source = (Image)args.Source;
        var pos = args.GetPosition(source);
        mouseDownCanvasPos = new() { X = pos.X / source.Width * ActiveDocument.Bitmaps[ChunkResolution.Full].PixelWidth, Y = pos.Y / source.Height * ActiveDocument.Bitmaps[ChunkResolution.Full].PixelHeight };
        toolOnMouseDown = activeTool;
        ProcessToolMouseDown(mouseDownCanvasPos);
    }

    private void ProcessToolMouseDown(VecD pos)
    {
        if (toolOnMouseDown is Tool.FloodFill)
        {
            ActiveDocument!.FloodFill((VecI)pos, new SKColor(SelectedColor.R, SelectedColor.G, SelectedColor.B, SelectedColor.A), FillAllLayers);
        }
        else if (toolOnMouseDown == Tool.PathBasedPen)
        {
            ActiveDocument!.StartUpdatePathBasedPen(pos);
        }
        else if (toolOnMouseDown == Tool.LineBasedPen)
        {
            ActiveDocument!.StartUpdateLineBasedPen((VecI)pos, new SKColor(SelectedColor.R, SelectedColor.G, SelectedColor.B, SelectedColor.A));
        }
        else if (toolOnMouseDown == Tool.PixelPerfectPen)
        {
            ActiveDocument!.StartUpdatePixelPerfectPen((VecI)pos, new SKColor(SelectedColor.R, SelectedColor.G, SelectedColor.B, SelectedColor.A));
        }
        else if (toolOnMouseDown == Tool.Eraser)
        {
            ActiveDocument!.StartUpdateLineBasedPen((VecI)pos, SKColors.Transparent, true);
        }
    }

    private void MouseMove(object? param)
    {
        if (!mouseIsDown)
            return;
        mouseHasMoved = true;
        var args = (MouseEventArgs)(param!);
        var source = (Image)args.Source;
        var pos = args.GetPosition(source);
        double curX = pos.X / source.Width * ActiveDocument!.Bitmaps[ChunkResolution.Full].PixelWidth;
        double curY = pos.Y / source.Height * ActiveDocument.Bitmaps[ChunkResolution.Full].PixelHeight;

        ProcessToolMouseMove(new VecD(curX, curY));
    }

    private void ProcessToolMouseMove(VecD canvasPos)
    {
        switch (toolOnMouseDown)
        {
            case Tool.Rectangle:
            {
                var rect = RectI.FromTwoPixels((VecI)mouseDownCanvasPos, (VecI)canvasPos);
                ActiveDocument!.StartUpdateRectangle(new ShapeData(
                    rect.Center,
                    rect.Size,
                    0,
                    (int)StrokeWidth,
                    new SKColor(SelectedColor.R, SelectedColor.G, SelectedColor.B, SelectedColor.A),
                    new SKColor(0, 0, 255, 128)));
                break;
            }
            case Tool.Ellipse:
            {
                ActiveDocument!.StartUpdateEllipse(
                    RectI.FromTwoPixels((VecI)mouseDownCanvasPos, (VecI)canvasPos),
                    new SKColor(SelectedColor.R, SelectedColor.G, SelectedColor.B, SelectedColor.A),
                    new SKColor(0, 0, 255, 128),
                    (int)StrokeWidth);
                break;
            }
            case Tool.SelectRectangle:
                ActiveDocument!.StartUpdateRectSelection(
                    RectI.FromTwoPixels((VecI)mouseDownCanvasPos, (VecI)canvasPos),
                    selectionMode);
                break;
            case Tool.SelectEllipse:
                ActiveDocument!.StartUpdateEllipseSelection(
                    RectI.FromTwoPixels((VecI)mouseDownCanvasPos, (VecI)canvasPos),
                    selectionMode);
                break;
            case Tool.ShiftLayer:
                ActiveDocument!.StartUpdateShiftLayer((VecI)canvasPos - (VecI)mouseDownCanvasPos);
                break;
            case Tool.Lasso:
                ActiveDocument!.StartUpdateLassoSelection((VecI)canvasPos, selectionMode);
                break;
            case Tool.PathBasedPen:
                ActiveDocument!.StartUpdatePathBasedPen(canvasPos);
                break;
            case Tool.LineBasedPen:
                ActiveDocument!.StartUpdateLineBasedPen((VecI)canvasPos, new SKColor(SelectedColor.R, SelectedColor.G, SelectedColor.B, SelectedColor.A));
                break;
            case Tool.PixelPerfectPen:
                ActiveDocument!.StartUpdatePixelPerfectPen((VecI)canvasPos, new SKColor(SelectedColor.R, SelectedColor.G, SelectedColor.B, SelectedColor.A));
                break;
            case Tool.Eraser:
                ActiveDocument!.StartUpdateLineBasedPen((VecI)canvasPos, SKColors.Transparent, true);
                break;
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
                case Tool.Ellipse:
                    ActiveDocument!.EndEllipse();
                    break;
                case Tool.SelectRectangle:
                    ActiveDocument!.EndRectSelection();
                    break;
                case Tool.SelectEllipse:
                    ActiveDocument!.EndEllipseSelection();
                    break;
                case Tool.ShiftLayer:
                    ActiveDocument!.EndShiftLayer();
                    break;
                case Tool.Lasso:
                    ActiveDocument!.EndLassoSelection();
                    break;
            }
        }

        switch (toolOnMouseDown)
        {
            case Tool.PathBasedPen:
                ActiveDocument!.EndPathBasedPen();
                break;
            case Tool.PixelPerfectPen:
                ActiveDocument!.EndUPixelPerfectPen();
                break;
            case Tool.LineBasedPen:
            case Tool.Eraser:
                ActiveDocument!.EndLineBasedPen();
                break;
        }
    }

    private void LoadDocument(object? args)
    {
        OpenFileDialog dialog = new();
        if (dialog.ShowDialog() != true)
            return;
        SerializableDocument serDocument;
        try
        {
            serDocument = PixiParser.Deserialize(dialog.FileName);
        }
        catch (Exception)
        {
            return;
        }

        DocumentViewModel document = DocumentViewModel.FromSerializableDocument(this, serDocument, Path.GetFileName(dialog.FileName));
        Documents.Add(document);
    }

    private void ChangeActiveTool(object? param)
    {
        if (param is null)
            return;
        activeTool = (Tool)param;
    }
}
