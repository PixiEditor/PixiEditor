using ChunkyImageLib.DataHolders;
using PixiEditor.Zoombox;
using PixiEditorPrototype.Models;
using PixiEditorPrototype.Views;
using SkiaSharp;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;

namespace PixiEditorPrototype.ViewModels
{
    internal class ViewModelMain : INotifyPropertyChanged
    {
        public IMainView? View { get; set; }
        public DocumentViewModel? ActiveDocument { get; }

        public RelayCommand? MouseDownCommand { get; }
        public RelayCommand? MouseMoveCommand { get; }
        public RelayCommand? MouseUpCommand { get; }
        public RelayCommand? ChangeActiveToolCommand { get; }

        public Color SelectedColor { get; set; } = Colors.Black;

        private bool mouseIsDown = false;
        private int mouseDownCanvasX = 0;
        private int mouseDownCanvasY = 0;

        private bool startedDrawingRect = false;
        private bool startedSelectingRect = false;

        private Tool activeTool = Tool.Rectangle;

        public event PropertyChangedEventHandler? PropertyChanged;

        private bool enableViewportDragging;
        public bool EnableViewportDragging
        {
            get => enableViewportDragging;
            set
            {
                enableViewportDragging = value;
                PropertyChanged?.Invoke(this, new(nameof(EnableViewportDragging)));
                PropertyChanged?.Invoke(this, new(nameof(ZoomboxMode)));
            }
        }

        public ZoomboxMode ZoomboxMode => enableViewportDragging ? ZoomboxMode.Move : ZoomboxMode.Normal;

        public ViewModelMain()
        {
            MouseDownCommand = new RelayCommand(MouseDown);
            MouseMoveCommand = new RelayCommand(MouseMove);
            MouseUpCommand = new RelayCommand(MouseUp);
            ChangeActiveToolCommand = new RelayCommand(ChangeActiveTool);

            ActiveDocument = new DocumentViewModel(this);
        }

        private void MouseDown(object? param)
        {
            if (ActiveDocument is null || EnableViewportDragging)
                return;
            mouseIsDown = true;
            var args = (MouseButtonEventArgs)(param!);
            var source = (System.Windows.Controls.Image)args.Source;
            var pos = args.GetPosition(source);
            mouseDownCanvasX = (int)(pos.X / source.Width * ActiveDocument.BitmapFull.PixelHeight);
            mouseDownCanvasY = (int)(pos.Y / source.Height * ActiveDocument.BitmapFull.PixelHeight);
        }

        private void MouseMove(object? param)
        {
            if (ActiveDocument is null || !mouseIsDown || EnableViewportDragging)
                return;
            var args = (MouseEventArgs)(param!);
            var source = (System.Windows.Controls.Image)args.Source;
            var pos = args.GetPosition(source);
            int curX = (int)(pos.X / source.Width * ActiveDocument.BitmapFull.PixelHeight);
            int curY = (int)(pos.Y / source.Height * ActiveDocument.BitmapFull.PixelHeight);

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
            if (ActiveDocument is null || !mouseIsDown || EnableViewportDragging)
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
}
