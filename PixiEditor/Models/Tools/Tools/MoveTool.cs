using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Undo;
using SkiaSharp;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PixiEditor.Models.Tools.Tools
{
    internal class MoveTool : BitmapOperationTool
    {
        private Layer[] affectedLayers;
        private Surface[] currentlyDragged;
        private Coordinates[] currentlyDraggedPositions;
        private Surface previewLayerData;

        private List<Coordinates> moveStartSelectedPoints = null;
        private Int32Rect moveStartRect;

        private Coordinates lastDragDelta;

        private StorageBasedChange change;

        private string defaultActionDisplay = "Hold mouse to move selected pixels. Hold Ctrl to move all layers.";

        public MoveTool(BitmapManager bitmapManager)
        {
            ActionDisplay = defaultActionDisplay;
            Cursor = Cursors.Arrow;
            RequiresPreviewLayer = true;
            UseDefaultUndoMethod = false;

            BitmapManager = bitmapManager;
        }

        public override string Tooltip => "Moves selected pixels (V). Hold Ctrl to move all layers.";

        public override bool HideHighlight => true;

        private BitmapManager BitmapManager { get; }

        public override void UpdateActionDisplay(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
        {
            if (ctrlIsDown)
                ActionDisplay = "Hold mouse to move all layers.";
            else
                ActionDisplay = defaultActionDisplay;
        }

        public override void BeforeUse()
        {
            Document doc = BitmapManager.ActiveDocument;
            Selection selection = doc.ActiveSelection;
            bool anySelection = selection.SelectedPoints.Any();

            if (Session.IsCtrlDown)
            {
                affectedLayers = doc.Layers.Where(x => x.IsVisible).ToArray();
            }
            else
            {
                affectedLayers = doc.Layers.Where(x => x.IsActive && doc.GetFinalLayerIsVisible(x)).ToArray();
            }

            change = new StorageBasedChange(doc, affectedLayers, true, true);

            Layer selLayer = selection.SelectionLayer;
            moveStartRect = anySelection ?
                new(selLayer.OffsetX, selLayer.OffsetY, selLayer.Width, selLayer.Height) :
                new(0, 0, doc.Width, doc.Height);
            lastDragDelta = new Coordinates(0, 0);

            previewLayerData?.Dispose();
            previewLayerData = CreateCombinedPreview(anySelection ? selLayer : null, affectedLayers);

            if (currentlyDragged != null)
            {
                foreach (var surface in currentlyDragged)
                    surface.Dispose();
            }

            if (anySelection)
            {
                currentlyDragged = BitmapUtils.ExtractSelectedPortions(selLayer, affectedLayers, true);
                currentlyDraggedPositions = Enumerable.Repeat(new Coordinates(selLayer.OffsetX, selLayer.OffsetY), affectedLayers.Length).ToArray();
            }
            else
            {
                (currentlyDraggedPositions, currentlyDragged) = CutDraggedLayers(affectedLayers);
            }

            if (anySelection)
                moveStartSelectedPoints = selection.SelectedPoints.ToList();
        }

        private Surface CreateCombinedPreview(Layer selLayer, Layer[] layersToCombine)
        {
            var combined = BitmapUtils.CombineLayers(moveStartRect, layersToCombine, BitmapManager.ActiveDocument.LayerStructure);
            if (selLayer != null)
            {
                using var selSnap = selLayer.LayerBitmap.SkiaSurface.Snapshot();
                combined.SkiaSurface.Canvas.DrawImage(selSnap, 0, 0, Surface.MaskingPaint);
            }
            return combined;
        }

        private static (Coordinates[], Surface[]) CutDraggedLayers(Layer[] draggedLayers)
        {
            Surface[] outSurfaces = new Surface[draggedLayers.Length];
            Coordinates[] outCoords = new Coordinates[draggedLayers.Length];

            int count = 0;
            foreach (var layer in draggedLayers)
            {
                outCoords[count] = new Coordinates(layer.OffsetX, layer.OffsetY);
                Surface copy = new(layer.Width, layer.Height);
                layer.LayerBitmap.SkiaSurface.Draw(copy.SkiaSurface.Canvas, 0, 0, Surface.ReplacingPaint);
                layer.LayerBitmap.SkiaSurface.Canvas.Clear();
                layer.InvokeLayerBitmapChange();
                outSurfaces[count] = copy;
                count++;
            }

            return (outCoords, outSurfaces);
        }

        public override void Use(Layer activeLayer, Layer previewLayer, IEnumerable<Layer> allLayers, IReadOnlyList<Coordinates> recordedMouseMovement, SKColor color)
        {
            Coordinates newPos = recordedMouseMovement[^1];
            Coordinates moveStartPos = recordedMouseMovement[0];
            int dX = newPos.X - moveStartPos.X;
            int dY = newPos.Y - moveStartPos.Y;
            BitmapManager.ActiveDocument.ActiveSelection.TranslateSelection(dX - lastDragDelta.X, dY - lastDragDelta.Y);
            lastDragDelta = new Coordinates(dX, dY);


            int newX = moveStartRect.X + dX;
            int newY = moveStartRect.Y + dY;

            Int32Rect dirtyRect = new Int32Rect(newX, newY, moveStartRect.Width, moveStartRect.Height);
            previewLayer.DynamicResizeAbsolute(dirtyRect);
            previewLayerData.SkiaSurface.Draw(previewLayer.LayerBitmap.SkiaSurface.Canvas, newX - previewLayer.OffsetX, newY - previewLayer.OffsetY, Surface.ReplacingPaint);
            previewLayer.InvokeLayerBitmapChange(dirtyRect);
        }

        public override void AfterUse(SKRectI sessionRect)
        {
            base.AfterUse(sessionRect);
            BitmapManager.ActiveDocument.PreviewLayer.ClearCanvas();

            ApplySurfacesToLayers(currentlyDragged, currentlyDraggedPositions, affectedLayers, new Coordinates(lastDragDelta.X, lastDragDelta.Y));
            foreach (var surface in currentlyDragged)
                surface.Dispose();
            currentlyDragged = null;

            SaveUndo(BitmapManager.ActiveDocument);
        }

        private static void ApplySurfacesToLayers(Surface[] surfaces, Coordinates[] startPositions, Layer[] layers, Coordinates delta)
        {
            int count = 0;
            foreach (Surface surface in surfaces)
            {
                var layer = layers[count];
                using SKImage snapshot = surface.SkiaSurface.Snapshot();
                Coordinates position = new Coordinates(startPositions[count].X + delta.X, startPositions[count].Y + delta.Y);
                Int32Rect dirtyRect = new Int32Rect(position.X, position.Y, surface.Width, surface.Height);
                layer.DynamicResizeAbsolute(dirtyRect);
                layer.LayerBitmap.SkiaSurface.Canvas.DrawImage(snapshot, position.X - layer.OffsetX, position.Y - layer.OffsetY);
                layer.InvokeLayerBitmapChange(dirtyRect);

                count++;
            }
        }

        private void SaveUndo(Document document)
        {
            var args = new object[] { change.Document };
            document.UndoManager.AddUndoChange(change.ToChange(StorageBasedChange.BasicUndoProcess, args));
            if (moveStartSelectedPoints != null)
            {
                SelectionHelpers.AddSelectionUndoStep(document, moveStartSelectedPoints, SelectionType.New);
                document.UndoManager.SquashUndoChanges(3, "Move selected area");
                moveStartSelectedPoints = null;
            }
            change = null;
        }
    }
}
