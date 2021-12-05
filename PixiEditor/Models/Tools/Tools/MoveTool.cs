using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.IO;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Undo;
using PixiEditor.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Transform = PixiEditor.Models.ImageManipulation.Transform;

namespace PixiEditor.Models.Tools.Tools
{
    public class MoveTool : BitmapOperationTool
    {
        private static readonly SKPaint maskingPaint = new()
        {
            BlendMode = SKBlendMode.DstIn,
        };

        private static readonly SKPaint inverseMaskingPaint = new()
        {
            BlendMode = SKBlendMode.DstOut,
        };

        private Layer[] affectedLayers;
        private Surface[] currentlyDragged;
        private Surface previewLayerData;
        
        private Coordinates moveStartPos;
        private Int32Rect moveStartRect;

        private Coordinates lastDragDelta;

        private StorageBasedChange change;

        //private Dictionary<Guid, bool> clearedPixels = new Dictionary<Guid, bool>();
        //private Coordinates[] currentSelection;
        //private Coordinates lastMouseMove;
        //private Dictionary<Guid, SKColor[]> startPixelColors;
        //private Dictionary<Guid, SKColor[]> endPixelColors;
        //private Dictionary<Guid, Thickness> startingOffsets;
        //private Coordinates[] startSelection;
        //private bool updateViewModelSelection = true;

        public MoveTool(BitmapManager bitmapManager)
        {
            ActionDisplay = "Hold mouse to move selected pixels. Hold Ctrl to move all layers.";
            Cursor = Cursors.Arrow;
            RequiresPreviewLayer = true;
            UseDefaultUndoMethod = false;

            BitmapManager = bitmapManager;
        }

        public override string Tooltip => "Moves selected pixels (V). Hold Ctrl to move all layers.";

        public override bool HideHighlight => true;

        public bool MoveAll { get; set; }

        private BitmapManager BitmapManager { get; }

        public override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
            {
                ActionDisplay = "Hold mouse to move all selected layers.";
            }
        }

        public override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
            {
                ActionDisplay = "Hold mouse to move selected pixels. Hold Ctrl to move all layers.";
            }
        }

        public void AfterAddedUndo(UndoManager undoManager)
        {
            //if (currentSelection == null || currentSelection.Length == 0)
            //{
            //    return;
            //}
            //Change changes = undoManager.UndoStack.Peek();
            //// Inject to default undo system change custom changes made by this tool
            //foreach (var item in startPixelColors)
            //{
            //    BitmapPixelChanges beforeMovePixels = BitmapPixelChanges.FromArrays(startSelection, item.Value);
            //    BitmapPixelChanges afterMovePixels = BitmapPixelChanges.FromArrays(currentSelection, endPixelColors[item.Key]);
            //    Guid layerGuid = item.Key;
            //    var oldValue = (LayerChange[])changes.OldValue;
            //    if (oldValue.Any(x => x.LayerGuid == layerGuid))
            //    {
            //        var layer = oldValue.First(x => x.LayerGuid == layerGuid);
            //        layer.PixelChanges.ChangedPixels.AddRangeOverride(afterMovePixels.ChangedPixels);
            //        layer.PixelChanges.ChangedPixels
            //            .AddRangeOverride(beforeMovePixels.ChangedPixels);
            //        ((LayerChange[])changes.NewValue).First(x => x.LayerGuid == layerGuid).PixelChanges.ChangedPixels
            //            .AddRangeNewOnly(BitmapPixelChanges
            //                .FromSingleColoredArray(startSelection, SKColors.Transparent)
            //                .ChangedPixels);
            //    }
            //}
        }

        // This adds undo if there is no selection, reason why this isn't in AfterUndoAdded,
        // is because it doesn't fire if no pixel changes were made.
        //public override void OnStoppedRecordingMouseUp(MouseEventArgs e)
        //{
        //    if (currentSelection != null && currentSelection.Length == 0)
        //    {
        //        BitmapManager.ActiveDocument.UndoManager.AddUndoChange(new Change(
        //            ApplyOffsets,
        //            new object[] { startingOffsets },
        //            ApplyOffsets,
        //            new object[] { GetOffsets(affectedLayers) },
        //            "Move layers"));
        //    }
        //}

        public override void AddUndoProcess(Document document)
        {
            var args = new object[] { change.Document };
            document.UndoManager.AddUndoChange(change.ToChange(UndoProcess, args));
            change = null;
        }

        private void UndoProcess(Layer[] layers, UndoLayer[] data, object[] args)
        {
            if (args.Length > 0 && args[0] is Document document)
            {
                for (int i = 0; i < layers.Length; i++)
                {
                    Layer layer = layers[i];
                    document.Layers.RemoveAt(data[i].LayerIndex);

                    document.Layers.Insert(data[i].LayerIndex, layer);
                    if (data[i].IsActive)
                    {
                        document.SetMainActiveLayer(data[i].LayerIndex);
                    }
                }

            }
        }

        public override void OnStart(Coordinates startPos)
        {
            //ResetSelectionValues(startPos);
            // Move offset if no selection
            Document doc = BitmapManager.ActiveDocument;
            Selection selection = doc.ActiveSelection;

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || MoveAll)
            {
                affectedLayers = doc.Layers.Where(x => x.IsVisible).ToArray();
            }
            else
            {
                affectedLayers = doc.Layers.Where(x => x.IsActive && doc.GetFinalLayerIsVisible(x)).ToArray();
            }

            change = new StorageBasedChange(doc, affectedLayers, true);

            Layer selLayer = selection.SelectionLayer;
            moveStartRect = new(selLayer.OffsetX, selLayer.OffsetY, selLayer.Width, selLayer.Height);
            moveStartPos = startPos;
            lastDragDelta = new Coordinates(0, 0);

            previewLayerData?.Dispose();
            previewLayerData = CreateCombinedPreview(selLayer, affectedLayers);

            if (currentlyDragged != null)
            {
                foreach (var surface in currentlyDragged)
                    surface.Dispose();
            }

            currentlyDragged = ExtractDraggedPortions(selLayer, affectedLayers);

            
            //startSelection = currentSelection;
            //startPixelColors = BitmapUtils.GetPixelsForSelection(affectedLayers, startSelection);
            //startingOffsets = GetOffsets(affectedLayers);
        }

        private Surface CreateCombinedPreview(Layer selLayer, Layer[] layersToCombine)
        {
            var combined = BitmapUtils.CombineLayers(moveStartRect, layersToCombine, BitmapManager.ActiveDocument.LayerStructure);
            using var selSnap = selLayer.LayerBitmap.SkiaSurface.Snapshot();
            combined.SkiaSurface.Canvas.DrawImage(selSnap, 0, 0, maskingPaint);
            return combined;
        }
        private static Surface[] ExtractDraggedPortions(Layer selLayer, Layer[] draggedLayers)
        {
            using var selSnap = selLayer.LayerBitmap.SkiaSurface.Snapshot();
            Surface[] output = new Surface[draggedLayers.Length];
            int count = 0;
            foreach (Layer layer in draggedLayers)
            {
                Surface portion = new Surface(selLayer.Width, selLayer.Height);
                SKRect selLayerRect = new SKRect(0, 0, selLayer.Width, selLayer.Height);

                int x = selLayer.OffsetX - layer.OffsetX;
                int y = selLayer.OffsetY - layer.OffsetY;

                using (var layerSnap = layer.LayerBitmap.SkiaSurface.Snapshot())
                    portion.SkiaSurface.Canvas.DrawImage(layerSnap, new SKRect(x, y, x + selLayer.Width, y + selLayer.Height), selLayerRect, Surface.ReplacingPaint);
                portion.SkiaSurface.Canvas.DrawImage(selSnap, 0, 0, maskingPaint);
                output[count] = portion;
                count++;

                layer.LayerBitmap.SkiaSurface.Canvas.DrawImage(selSnap, new SKRect(0, 0, selLayer.Width, selLayer.Height), 
                    new SKRect(selLayer.OffsetX-layer.OffsetX, selLayer.OffsetY-layer.OffsetY, selLayer.OffsetX - layer.OffsetX + selLayer.Width, selLayer.OffsetY - layer.OffsetY + selLayer.Height), 
                    inverseMaskingPaint);
                layer.InvokeLayerBitmapChange(new Int32Rect(selLayer.OffsetX, selLayer.OffsetY, selLayer.Width, selLayer.Height));
            }
            return output;
        }

        public override void Use(Layer layer, List<Coordinates> mouseMove, SKColor color)
        {
            //LayerChange[] result = new LayerChange[affectedLayers.Length];


            Coordinates newPos = mouseMove[0];
            int dX = newPos.X - moveStartPos.X;
            int dY = newPos.Y - moveStartPos.Y;
            BitmapManager.ActiveDocument.ActiveSelection.TranslateSelection(dX - lastDragDelta.X, dY - lastDragDelta.Y);
            lastDragDelta = new Coordinates(dX, dY);


            int newX = moveStartRect.X + dX;
            int newY = moveStartRect.Y + dY;
            
            layer.DynamicResizeAbsolute(newX + moveStartRect.Width, newY + moveStartRect.Height, newX, newY);
            previewLayerData.SkiaSurface.Draw(layer.LayerBitmap.SkiaSurface.Canvas, newX - layer.OffsetX, newY - layer.OffsetY, Surface.ReplacingPaint);
            layer.InvokeLayerBitmapChange(new Int32Rect(newX, newY, moveStartRect.Width, moveStartRect.Height));

            

            /*var end = mouseMove[0];
            var lastSelection = currentSelection.ToArray();
            for (int i = 0; i < affectedLayers.Length; i++)
            {
                if (currentSelection.Length > 0)
                {
                    endPixelColors = BitmapUtils.GetPixelsForSelection(affectedLayers, currentSelection);
                    var changes = MoveSelection(affectedLayers[i], mouseMove);
                    ClearSelectedPixels(affectedLayers[i], lastSelection);

                    changes = RemoveTransparentPixels(changes);

                    result[i] = new LayerChange(changes, affectedLayers[i]);
                }
                else
                {
                    var vector = Transform.GetTranslation(lastMouseMove, end);
                    affectedLayers[i].Offset = new Thickness(affectedLayers[i].OffsetX + vector.X, affectedLayers[i].OffsetY + vector.Y, 0, 0);
                    result[i] = new LayerChange(BitmapPixelChanges.Empty, affectedLayers[i]);
                }
            }

            lastMouseMove = end;

            return result;*/
        }

        public override void OnStoppedRecordingMouseUp(MouseEventArgs e)
        {
            base.OnStoppedRecordingMouseUp(e);

            BitmapManager.ActiveDocument.PreviewLayer.ClearCanvas();

            ApplySurfacesToLayers(currentlyDragged, affectedLayers, new Coordinates(moveStartRect.X + lastDragDelta.X, moveStartRect.Y + lastDragDelta.Y));
            foreach (var surface in currentlyDragged)
                surface.Dispose();
            currentlyDragged = null;
        }

        private static void ApplySurfacesToLayers(Surface[] surfaces, Layer[] layers, Coordinates position)
        {
            int count = 0;
            foreach (Surface surface in surfaces)
            {
                var layer = layers[count];
                using SKImage snapshot = surface.SkiaSurface.Snapshot();
                layer.DynamicResizeAbsolute(position.X + surface.Width, position.Y + surface.Height, position.X, position.Y);
                layer.LayerBitmap.SkiaSurface.Canvas.DrawImage(snapshot, position.X - layer.OffsetX, position.Y - layer.OffsetY);
                layer.InvokeLayerBitmapChange(new Int32Rect(position.X, position.Y, surface.Width, surface.Height));

                count++;
            }
        }

        //public BitmapPixelChanges MoveSelection(Layer layer, IEnumerable<Coordinates> mouseMove)
        //{
        //    Coordinates end = mouseMove.First();

        //    currentSelection = TranslateSelection(end);
        //    if (updateViewModelSelection)
        //    {
        //        ViewModelMain.Current.BitmapManager.ActiveDocument.ActiveSelection.SetSelection(currentSelection, SelectionType.New);
        //    }
        //    lastMouseMove = end;
        //    return BitmapPixelChanges.FromArrays(currentSelection, startPixelColors[layer.LayerGuid]);
        //}
        //private void ApplyOffsets(object[] parameters)
        //{
        //    Dictionary<Guid, Thickness> offsets = (Dictionary<Guid, Thickness>)parameters[0];
        //    foreach (var offset in offsets)
        //    {
        //        Layer layer = ViewModelMain.Current?.BitmapManager?.
        //            ActiveDocument?.Layers?.First(x => x.LayerGuid == offset.Key);
        //        layer.Offset = offset.Value;
        //    }
        //}

        //private Dictionary<Guid, Thickness> GetOffsets(Layer[] layers)
        //{
        //    Dictionary<Guid, Thickness> dict = new Dictionary<Guid, Thickness>();
        //    for (int i = 0; i < layers.Length; i++)
        //    {
        //        dict.Add(layers[i].LayerGuid, layers[i].Offset);
        //    }

        //    return dict;
        //}

        //private BitmapPixelChanges RemoveTransparentPixels(BitmapPixelChanges pixels)
        //{
        //    foreach (var item in pixels.ChangedPixels.Where(x => x.Value.Alpha == 0).ToList())
        //    {
        //        pixels.ChangedPixels.Remove(item.Key);
        //    }
        //    return pixels;
        //}

        //private void ResetSelectionValues(Coordinates start)
        //{
        //    lastMouseMove = start;
        //    clearedPixels = new Dictionary<Guid, bool>();
        //    endPixelColors = new Dictionary<Guid, SKColor[]>();
        //    currentSelection = null;
        //    affectedLayers = null;
        //    updateViewModelSelection = true;
        //    startPixelColors = null;
        //    startSelection = null;
        //}

        //private Coordinates[] TranslateSelection(Coordinates end)
        //{
        //    Coordinates translation = Transform.GetTranslation(lastMouseMove, end);
        //    return Transform.Translate(currentSelection, translation);
        //}

        //private void ClearSelectedPixels(Layer layer, Coordinates[] selection)
        //{
        //    Guid layerGuid = layer.LayerGuid;
        //    if (!clearedPixels.ContainsKey(layerGuid) || clearedPixels[layerGuid] == false)
        //    {
        //        BitmapManager.ActiveDocument.Layers.First(x => x == layer)
        //            .SetPixels(BitmapPixelChanges.FromSingleColoredArray(selection, SKColors.Transparent));

        //        clearedPixels[layerGuid] = true;
        //    }
        //}
    }
}
