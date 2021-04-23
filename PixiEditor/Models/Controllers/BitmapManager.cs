using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Events;
using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
using PixiEditor.Models.Tools.Tools;
using PixiEditor.Models.Tools.ToolSettings.Settings;

namespace PixiEditor.Models.Controllers
{
    [DebuggerDisplay("{Documents.Count} Document(s)")]
    public class BitmapManager : NotifyableObject
    {
        private Document activeDocument;
        private Tool selectedTool;
        private Coordinates? startPosition = null;

        public BitmapManager()
        {
            MouseController = new MouseMovementController();
            MouseController.StartedRecordingChanges += MouseController_StartedRecordingChanges;
            MouseController.MousePositionChanged += Controller_MousePositionChanged;
            MouseController.StoppedRecordingChanges += MouseController_StoppedRecordingChanges;
            MouseController.OnMouseDown += MouseController_OnMouseDown;
            MouseController.OnMouseUp += MouseController_OnMouseUp;
            BitmapOperations = new BitmapOperationsUtility(this);
            ReadonlyToolUtility = new ReadonlyToolUtility();
        }

        public event EventHandler<DocumentChangedEventArgs> DocumentChanged;

        public MouseMovementController MouseController { get; set; }

        public Tool SelectedTool
        {
            get => selectedTool;
            private set
            {
                selectedTool = value;
                RaisePropertyChanged("SelectedTool");
            }
        }

        public Layer ActiveLayer => ActiveDocument.ActiveLayer;

        public Color PrimaryColor { get; set; }

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

                if (IsOperationTool(SelectedTool))
                {
                    BitmapOperations.ExecuteTool(newPosition, MouseController.LastMouseMoveCoordinates, (BitmapOperationTool)SelectedTool);
                }
                else
                {
                    ReadonlyToolUtility.ExecuteTool(MouseController.LastMouseMoveCoordinates, (ReadonlyTool)SelectedTool);
                }
            }
        }

        public WriteableBitmap GetCombinedLayersBitmap()
        {
            return BitmapUtils.CombineLayers(ActiveDocument.Width, ActiveDocument.Height, ActiveDocument.Layers.Where(x => x.IsVisible).ToArray());
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
            if (ActiveDocument != null)
            {
                ActiveDocument.PreviewLayer = null;
            }

            SelectedTool?.Toolbar.SaveToolbarSettings();
            SelectedTool = tool;
            SelectedTool.Toolbar.LoadSharedSettings();
        }

        private void Controller_MousePositionChanged(object sender, MouseMovementEventArgs e)
        {
            SelectedTool.OnMouseMove(new MouseEventArgs(Mouse.PrimaryDevice, (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
            if (Mouse.LeftButton == MouseButtonState.Pressed && !IsDraggingViewport() && ActiveDocument != null)
            {
                ExecuteTool(e.NewPosition, MouseController.ClickedOnCanvas);
            }
            else if (Mouse.LeftButton == MouseButtonState.Released)
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

        private bool IsDraggingViewport()
        {
            return SelectedTool is MoveViewportTool;
        }

        private void MouseController_StartedRecordingChanges(object sender, EventArgs e)
        {
            SelectedTool.OnRecordingLeftMouseDown(new MouseEventArgs(Mouse.PrimaryDevice, (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
            if (ActiveDocument != null)
            {
                ActiveDocument.PreviewLayer = null;
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

            IEnumerable<Coordinates> highlightArea = CoordinatesCalculator.RectangleToCoordinates(
                CoordinatesCalculator.CalculateThicknessCenter(newPosition, ToolSize));
            if (CanChangeHighlightOffset(highlightArea))
            {
                Coordinates start = highlightArea.First();
                ActiveDocument.PreviewLayer.Offset = new Thickness(start.X, start.Y, 0, 0);
            }
            else if (!IsInsideBounds(highlightArea))
            {
                ActiveDocument.PreviewLayer = null;
            }
            else
            {
                ActiveDocument.GeneratePreviewLayer();
                ActiveDocument.PreviewLayer.SetPixels(
                    BitmapPixelChanges.FromSingleColoredArray(highlightArea, Color.FromArgb(77, 0, 0, 0)));
            }
        }

        private bool CanChangeHighlightOffset(IEnumerable<Coordinates> highlightArea)
        {
            int count = highlightArea.Count();
            return count > 0 && ActiveDocument.PreviewLayer != null &&
                   IsInsideBounds(highlightArea) && count == ActiveDocument.PreviewLayer.Width * ActiveDocument.PreviewLayer.Height;
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