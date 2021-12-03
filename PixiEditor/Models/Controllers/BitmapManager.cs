using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Events;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
using PixiEditor.Models.Tools.Tools;
using PixiEditor.Models.Tools.ToolSettings.Settings;
using PixiEditor.ViewModels;
using PixiEditor.ViewModels.SubViewModels.Main;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PixiEditor.Models.Controllers
{
    [DebuggerDisplay("{Documents.Count} Document(s)")]
    public class BitmapManager : NotifyableObject
    {
        private readonly ToolsViewModel _tools;

        private int previewLayerSize;
        private Document activeDocument;
        private Coordinates? startPosition = null;
        private int halfSize;
        private SKColor _highlightColor;
        private PenTool _highlightPen;
        private bool hideReferenceLayer;
        private bool onlyReferenceLayer;

        public BitmapManager(ToolsViewModel tools)
        {
            _tools = tools;

            MouseController = new MouseMovementController();
            MouseController.StartedRecordingChanges += MouseController_StartedRecordingChanges;
            MouseController.MousePositionChanged += Controller_MousePositionChanged;
            MouseController.StoppedRecordingChanges += MouseController_StoppedRecordingChanges;
            MouseController.OnMouseDown += MouseController_OnMouseDown;
            MouseController.OnMouseUp += MouseController_OnMouseUp;
            MouseController.OnMouseDownCoordinates += MouseController_OnMouseDownCoordinates;
            BitmapOperations = new BitmapOperationsUtility(this, tools);
            ReadonlyToolUtility = new ReadonlyToolUtility();
            DocumentChanged += BitmapManager_DocumentChanged;
            _highlightPen = new PenTool(this)
            {
                AutomaticallyResizeCanvas = false
            };
            _highlightColor = new SKColor(0, 0, 0, 77);
        }

        public event EventHandler<DocumentChangedEventArgs> DocumentChanged;

        public MouseMovementController MouseController { get; set; }

        public Layer ActiveLayer => ActiveDocument.ActiveLayer;

        public SKColor PrimaryColor { get; set; }

        public BitmapOperationsUtility BitmapOperations { get; set; }

        public ReadonlyToolUtility ReadonlyToolUtility { get; set; }

#nullable enable
        public Document ActiveDocument
        {
            get => activeDocument;
            set
            {
                activeDocument?.UpdatePreviewImage();
                Document? oldDoc = activeDocument;
                activeDocument = value;
                RaisePropertyChanged(nameof(ActiveDocument));
                DocumentChanged?.Invoke(this, new DocumentChangedEventArgs(value, oldDoc));
            }
        }

#nullable disable
        public ObservableCollection<Document> Documents { get; set; } = new ObservableCollection<Document>();

        public bool HideReferenceLayer
        {
            get => hideReferenceLayer;
            set => SetProperty(ref hideReferenceLayer, value);
        }

        public bool OnlyReferenceLayer
        {
            get => onlyReferenceLayer;
            set => SetProperty(ref onlyReferenceLayer, value);
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

        public void ExecuteTool(Coordinates newPosition, bool clickedOnCanvas)
        {
            Tool activeTool = _tools.ActiveTool;

            if (activeTool.CanStartOutsideCanvas && !clickedOnCanvas)
            {
                return;
            }

            if (startPosition == null)
            {
                activeTool.OnStart(newPosition);
                startPosition = newPosition;
            }

            if (activeTool is BitmapOperationTool operationTool)
            {
                BitmapOperations.ExecuteTool(newPosition, MouseController.LastMouseMoveCoordinates, operationTool);
            }
            else if (activeTool is ReadonlyTool readonlyTool)
            {
                ReadonlyToolUtility.ExecuteTool(
                    MouseController.LastMouseMoveCoordinates,
                    readonlyTool);
            }
            else
            {
                throw new InvalidOperationException($"'{activeTool.GetType().Name}' is either not a Tool or can't inherit '{nameof(Tool)}' directly.\nChanges the base type to either '{nameof(BitmapOperationTool)}' or '{nameof(ReadonlyTool)}'");
            }
        }

        public void HighlightPixels(Coordinates newPosition)
        {
            if (ActiveDocument == null || ActiveDocument.Layers.Count == 0 || _tools.ActiveTool.HideHighlight)
            {
                return;
            }

            var previewLayer = ActiveDocument.PreviewLayer;

            if (_tools.ToolSize != previewLayerSize || previewLayer.IsReset)
            {
                previewLayerSize = _tools.ToolSize;
                halfSize = (int)Math.Floor(_tools.ToolSize / 2f);
                previewLayer.CreateNewBitmap(_tools.ToolSize, _tools.ToolSize);

                Coordinates cords = new Coordinates(halfSize, halfSize);

                previewLayer.Offset = new Thickness(0, 0, 0, 0);
                _highlightPen.Draw(previewLayer, cords, cords, _highlightColor, _tools.ToolSize);

                AdjustOffset(newPosition, previewLayer);

            }

            previewLayer.InvokeLayerBitmapChange();

            AdjustOffset(newPosition, previewLayer);

            if (newPosition.X > ActiveDocument.Width
                || newPosition.Y > ActiveDocument.Height
                || newPosition.X < 0 || newPosition.Y < 0)
            {
                previewLayer.Reset();
                previewLayerSize = -1;
            }
        }

        private void BitmapManager_DocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            e.NewDocument?.GeneratePreviewLayer();
        }

        private void Controller_MousePositionChanged(object sender, MouseMovementEventArgs e)
        {
            Tool activeTool = _tools.ActiveTool;

            if (activeTool == null)
            {
                return;
            }

            activeTool.OnMouseMove(new MouseEventArgs(Mouse.PrimaryDevice, (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
            if (!MaybeExecuteTool(e.NewPosition) && Mouse.LeftButton == MouseButtonState.Released)
            {
                HighlightPixels(e.NewPosition);
            }
        }

        private void MouseController_OnMouseDown(object sender, MouseEventArgs e)
        {
            _tools.ActiveTool.OnMouseDown(e);
        }

        private void MouseController_OnMouseUp(object sender, MouseEventArgs e)
        {
            _tools.ActiveTool.OnMouseUp(e);
        }
        private void MouseController_OnMouseDownCoordinates(object sender, MouseMovementEventArgs e)
        {
            MaybeExecuteTool(e.NewPosition);
        }

        private bool MaybeExecuteTool(Coordinates newPosition)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed && !IsDraggingViewport() && ActiveDocument != null)
            {
                ExecuteTool(newPosition, MouseController.ClickedOnCanvas);
                return true;
            }
            return false;
        }

        private bool IsDraggingViewport()
        {
            return _tools.ActiveTool is MoveViewportTool;
        }

        private void MouseController_StartedRecordingChanges(object sender, EventArgs e)
        {
            _tools.ActiveTool.OnRecordingLeftMouseDown(new MouseEventArgs(Mouse.PrimaryDevice, (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
            if (ActiveDocument != null)
            {
                ActiveDocument.PreviewLayer.Reset();
            }
        }

        private void MouseController_StoppedRecordingChanges(object sender, EventArgs e)
        {
            Tool selectedTool = _tools.ActiveTool;
            selectedTool.OnStoppedRecordingMouseUp(new MouseEventArgs(Mouse.PrimaryDevice, (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
            if (selectedTool is BitmapOperationTool operationTool && operationTool.RequiresPreviewLayer)
            {
                BitmapOperations.ApplyPreviewLayer();
            }

            HighlightPixels(MousePositionConverter.CurrentCoordinates);

            startPosition = null;
        }

        private void AdjustOffset(Coordinates newPosition, Layer previewLayer)
        {
            Coordinates start = newPosition - halfSize;
            previewLayer.Offset = new Thickness(start.X, start.Y, 0, 0);
        }
    }
}
