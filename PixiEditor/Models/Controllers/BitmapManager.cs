using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Events;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
using PixiEditor.Models.Tools.Tools;
using PixiEditor.ViewModels.SubViewModels.Main;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace PixiEditor.Models.Controllers
{
    [DebuggerDisplay("{Documents.Count} Document(s)")]
    public class BitmapManager : NotifyableObject
    {
        private ToolSessionController ToolSessionController { get; set; }
        public ICanvasInputTarget InputTarget => ToolSessionController;
        public BitmapOperationsUtility BitmapOperations { get; set; }

        public ObservableCollection<Document> Documents { get; set; } = new ObservableCollection<Document>();

        private Document activeDocument;
        public Document ActiveDocument
        {
            get => activeDocument;
            set
            {
                if (activeDocument == value)
                    return;
                activeDocument?.UpdatePreviewImage();
                Document oldDoc = activeDocument;
                activeDocument = value;
                RaisePropertyChanged(nameof(ActiveDocument));
                ActiveWindow = value;
                DocumentChanged?.Invoke(this, new DocumentChangedEventArgs(value, oldDoc));
            }
        }

        private object activeWindow;
        public object ActiveWindow
        {
            get => activeWindow;
            set
            {
                if (activeWindow == value)
                    return;
                activeWindow = value;
                RaisePropertyChanged(nameof(ActiveWindow));
                if (activeWindow is Document doc)
                    ActiveDocument = doc;
            }
        }

        public event EventHandler<DocumentChangedEventArgs> DocumentChanged;
        public event EventHandler StopUsingTool;

        public Layer ActiveLayer => ActiveDocument.ActiveLayer;

        public SKColor PrimaryColor { get; set; }

        private bool hideReferenceLayer;
        public bool HideReferenceLayer
        {
            get => hideReferenceLayer;
            set => SetProperty(ref hideReferenceLayer, value);
        }

        private bool onlyReferenceLayer;
        public bool OnlyReferenceLayer
        {
            get => onlyReferenceLayer;
            set => SetProperty(ref onlyReferenceLayer, value);
        }

        private readonly ToolsViewModel _tools;

        private int previewLayerSize;
        private int halfSize;
        private SKColor _highlightColor;
        private PenTool _highlightPen;

        private ToolSession activeSession = null;


        public BitmapManager(ToolsViewModel tools, UndoViewModel undo)
        {
            _tools = tools;

            ToolSessionController = new ToolSessionController();
            ToolSessionController.SessionStarted += OnSessionStart;
            ToolSessionController.SessionEnded += OnSessionEnd;
            ToolSessionController.PixelMousePositionChanged += OnPixelMousePositionChange;
            ToolSessionController.PreciseMousePositionChanged += OnPreciseMousePositionChange;
            ToolSessionController.KeyStateChanged += (_, _) => UpdateActionDisplay(_tools.ActiveTool);
            BitmapOperations = new BitmapOperationsUtility(this, tools);

            undo.UndoRedoCalled += (_, _) => ToolSessionController.ForceStopActiveSessionIfAny();

            DocumentChanged += BitmapManager_DocumentChanged;

            _highlightPen = new PenTool(this)
            {
                AutomaticallyResizeCanvas = false
            };
            _highlightColor = new SKColor(0, 0, 0, 77);
        }

        public void CloseDocument(Document document)
        {
            int nextIndex = 0;
            if (document == ActiveDocument)
            {
                nextIndex = Documents.Count > 1 ? Documents.IndexOf(document) : -1;
                nextIndex += nextIndex > 0 ? -1 : 0;
            }

            Documents.Remove(document);
            ActiveDocument = nextIndex >= 0 ? Documents[nextIndex] : null;
            document.Dispose();
        }

        public void UpdateActionDisplay(Tool tool)
        {
            tool?.UpdateActionDisplay(ToolSessionController.IsCtrlDown, ToolSessionController.IsShiftDown, ToolSessionController.IsAltDown);
        }

        private void OnSessionStart(object sender, ToolSession e)
        {
            activeSession = e;

            ActiveDocument.PreviewLayer.Reset();
            ExecuteTool();
        }

        private void OnSessionEnd(object sender, ToolSession e)
        {
            activeSession = null;

            if (e.Tool is BitmapOperationTool operationTool && operationTool.RequiresPreviewLayer)
            {
                BitmapOperations.ApplyPreviewLayer();
            }

            ActiveDocument.PreviewLayer.Reset();
            HighlightPixels(ToolSessionController.LastPixelPosition);
            StopUsingTool?.Invoke(this, EventArgs.Empty);
        }

        private void OnPreciseMousePositionChange(object sender, (double, double) e)
        {
            if (activeSession == null || !activeSession.Tool.RequiresPreciseMouseData)
                return;
            ExecuteTool();
        }

        private void OnPixelMousePositionChange(object sender, MouseMovementEventArgs e)
        {
            if (activeSession != null)
            {
                if (activeSession.Tool.RequiresPreciseMouseData)
                    return;
                ExecuteTool();
                return;
            }
            else
            {
                HighlightPixels(e.NewPosition);
            }
        }

        private void ExecuteTool()
        {
            if (activeSession == null)
                throw new Exception("Can't execute tool's Use outside a session");

            if (!activeSession.MouseMovement.Any())
                return;

            if (activeSession.Tool is BitmapOperationTool operationTool)
            {
                BitmapOperations.UseTool(activeSession.MouseMovement, operationTool, PrimaryColor);
            }
            else if (activeSession.Tool is ReadonlyTool readonlyTool)
            {
                readonlyTool.Use(activeSession.MouseMovement);
            }
            else
            {
                throw new InvalidOperationException($"'{activeSession.Tool.GetType().Name}' is either not a Tool or can't inherit '{nameof(Tool)}' directly.\nChanges the base type to either '{nameof(BitmapOperationTool)}' or '{nameof(ReadonlyTool)}'");
            }
        }

        private void BitmapManager_DocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            e.NewDocument?.GeneratePreviewLayer();
            if (e.OldDocument != e.NewDocument)
                ToolSessionController.ForceStopActiveSessionIfAny();
        }

        public void UpdateHighlightIfNecessary(bool forceHide = false)
        {
            if (activeSession != null)
                return;

            HighlightPixels(forceHide ? new(-1, -1) : ToolSessionController.LastPixelPosition);
        }

        private void HighlightPixels(Coordinates newPosition)
        {
            if (ActiveDocument == null || ActiveDocument.Layers.Count == 0)
            {
                return;
            }

            var previewLayer = ActiveDocument.PreviewLayer;

            if (newPosition.X > ActiveDocument.Width
                || newPosition.Y > ActiveDocument.Height
                || newPosition.X < 0 || newPosition.Y < 0
                || _tools.ActiveTool.HideHighlight)
            {
                previewLayer.Reset();
                previewLayerSize = -1;
                return;
            }

            if (_tools.ToolSize != previewLayerSize || previewLayer.IsReset)
            {
                previewLayerSize = _tools.ToolSize;
                halfSize = (int)Math.Floor(_tools.ToolSize / 2f);
                previewLayer.CreateNewBitmap(_tools.ToolSize, _tools.ToolSize);

                Coordinates cords = new Coordinates(halfSize, halfSize);

                previewLayer.Offset = new Thickness(0, 0, 0, 0);
                _highlightPen.Draw(previewLayer, cords, cords, _highlightColor, _tools.ToolSize);
            }
            AdjustOffset(newPosition, previewLayer);

            previewLayer.InvokeLayerBitmapChange();
        }

        private void AdjustOffset(Coordinates newPosition, Layer previewLayer)
        {
            Coordinates start = newPosition - halfSize;
            previewLayer.Offset = new Thickness(start.X, start.Y, 0, 0);
        }
    }
}
