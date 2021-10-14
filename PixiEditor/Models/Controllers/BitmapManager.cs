using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Events;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
using PixiEditor.Models.Tools.Tools;
using PixiEditor.Models.Tools.ToolSettings.Settings;
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
        private int previewLayerSize;
        private Document activeDocument;
        private Tool selectedTool;
        private Coordinates? startPosition = null;
        private IEnumerable<Coordinates> cachedHighlight;
        private int halfSize;
        private BitmapPixelChanges cachedPixels;

        public BitmapManager()
        {
            MouseController = new MouseMovementController();
            MouseController.StartedRecordingChanges += MouseController_StartedRecordingChanges;
            MouseController.MousePositionChanged += Controller_MousePositionChanged;
            MouseController.StoppedRecordingChanges += MouseController_StoppedRecordingChanges;
            MouseController.OnMouseDown += MouseController_OnMouseDown;
            MouseController.OnMouseUp += MouseController_OnMouseUp;
            MouseController.OnMouseDownCoordinates += MouseController_OnMouseDownCoordinates;
            BitmapOperations = new BitmapOperationsUtility(this);
            ReadonlyToolUtility = new ReadonlyToolUtility();
            DocumentChanged += BitmapManager_DocumentChanged;
        }

        public event EventHandler<DocumentChangedEventArgs> DocumentChanged;

        public event EventHandler<SelectedToolEventArgs> SelectedToolChanged;

        public MouseMovementController MouseController { get; set; }

        public Tool SelectedTool
        {
            get => selectedTool;
            private set
            {
                Tool previousTool = selectedTool;
                if (SetProperty(ref selectedTool, value))
                {
                    SelectedToolChanged?.Invoke(this, new SelectedToolEventArgs(previousTool, value));
                }
            }
        }

        public Layer ActiveLayer => ActiveDocument.ActiveLayer;

        public SKColor PrimaryColor { get; set; }

        public int ToolSize
        {
            get => SelectedTool.Toolbar.GetSetting<SizeSetting>("ToolSize") != null
            ? SelectedTool.Toolbar.GetSetting<SizeSetting>("ToolSize").Value
            : 1;
            set
            {
                if (SelectedTool.Toolbar.GetSetting<SizeSetting>("ToolSize") is SizeSetting toolSize)
                {
                    toolSize.Value = value;
                    HighlightPixels(MousePositionConverter.CurrentCoordinates);
                }
            }
        }

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

        /// <summary>
        ///     Returns if tool is BitmapOperationTool.
        /// </summary>
        public static bool IsOperationTool(Tool tool)
        {
            return tool is BitmapOperationTool;
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
        }

        public void ExecuteTool(Coordinates newPosition, bool clickedOnCanvas)
        {
            if (SelectedTool.CanStartOutsideCanvas || clickedOnCanvas)
            {
                if (startPosition == null)
                {
                    SelectedTool.OnStart(newPosition);
                    startPosition = newPosition;
                }

                if (SelectedTool is BitmapOperationTool operationTool)
                {
                    BitmapOperations.ExecuteTool(newPosition, MouseController.LastMouseMoveCoordinates, operationTool);
                }
                else if (SelectedTool is ReadonlyTool readonlyTool)
                {
                    ReadonlyToolUtility.ExecuteTool(
                        MouseController.LastMouseMoveCoordinates,
                        readonlyTool);
                }
                else
                {
                    throw new InvalidOperationException($"'{SelectedTool.GetType().Name}' is either not a Tool or can't inherit '{nameof(Tool)}' directly.\nChanges the base type to either '{nameof(BitmapOperationTool)}' or '{nameof(ReadonlyTool)}'");
                }
            }
        }

        /// <summary>
        ///     Returns if selected tool is BitmapOperationTool.
        /// </summary>
        public bool IsOperationTool()
        {
            return IsOperationTool(SelectedTool);
        }

        public void SetActiveTool(Tool tool)
        {
            ActiveDocument?.PreviewLayer?.Clear();
            SelectedTool?.Toolbar.SaveToolbarSettings();
            SelectedTool = tool;
            SelectedTool.Toolbar.LoadSharedSettings();
        }

        private void BitmapManager_DocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            e.NewDocument?.GeneratePreviewLayer();
        }

        private void Controller_MousePositionChanged(object sender, MouseMovementEventArgs e)
        {
            SelectedTool.OnMouseMove(new MouseEventArgs(Mouse.PrimaryDevice, (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
            if (!MaybeExecuteTool(e.NewPosition) && Mouse.LeftButton == MouseButtonState.Released)
            {
                HighlightPixels(e.NewPosition);
            }
        }

        private void MouseController_OnMouseDown(object sender, MouseEventArgs e)
        {
            SelectedTool.OnMouseDown(e);
        }

        private void MouseController_OnMouseUp(object sender, MouseEventArgs e)
        {
            SelectedTool.OnMouseUp(e);
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
            return SelectedTool is MoveViewportTool;
        }

        private void MouseController_StartedRecordingChanges(object sender, EventArgs e)
        {
            SelectedTool.OnRecordingLeftMouseDown(new MouseEventArgs(Mouse.PrimaryDevice, (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
            if (ActiveDocument != null)
            {
                ActiveDocument.PreviewLayer.Clear();
            }
        }

        private void MouseController_StoppedRecordingChanges(object sender, EventArgs e)
        {
            SelectedTool.OnStoppedRecordingMouseUp(new MouseEventArgs(Mouse.PrimaryDevice, (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
            if (IsOperationTool(SelectedTool) && ((BitmapOperationTool)SelectedTool).RequiresPreviewLayer)
            {
                BitmapOperations.ApplyPreviewLayer();
            }

            startPosition = null;
        }

        private void HighlightPixels(Coordinates newPosition)
        {
            if (ActiveDocument == null || ActiveDocument.Layers.Count == 0 || SelectedTool.HideHighlight)
            {
                return;
            }

            if (ToolSize != previewLayerSize || ActiveDocument.PreviewLayer.IsCleared)
            {
                cachedHighlight = CoordinatesCalculator.RectangleToCoordinates(
                    CoordinatesCalculator.CalculateThicknessCenter(newPosition, ToolSize));

                previewLayerSize = ToolSize;
                halfSize = (int)Math.Floor(ToolSize / 2f);

                cachedPixels = BitmapPixelChanges.FromSingleColoredArray(cachedHighlight, new SKColor(0, 0, 0, 77));

                if (!ActiveDocument.PreviewLayer.IsCleared)
                    ActiveDocument.PreviewLayer.Clear();
                ActiveDocument.PreviewLayer.SetPixels(cachedPixels);
            }

            Coordinates start = newPosition - halfSize;
            ActiveDocument.PreviewLayer.Offset = new Thickness(start.X, start.Y, 0, 0);

            if (!IsInsideBounds(cachedHighlight))
            {
                ActiveDocument.PreviewLayer.Clear();
                previewLayerSize = -1;
            }
        }

        private bool IsInsideBounds(IEnumerable<Coordinates> highlightArea)
        {
            Coordinates start = highlightArea.First();
            Coordinates end = highlightArea.Last();
            return start.X <= ActiveDocument.Width - 1 &&
                    start.Y <= ActiveDocument.Height - 1 &&
                   end.X >= 0 && end.Y >= 0;
        }
    }
}
