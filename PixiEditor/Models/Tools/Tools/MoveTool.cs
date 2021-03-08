using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Undo;
using PixiEditor.ViewModels;
using Transform = PixiEditor.Models.ImageManipulation.Transform;

namespace PixiEditor.Models.Tools.Tools
{
    public class MoveTool : BitmapOperationTool
    {
        private Layer[] affectedLayers;
        private Dictionary<Guid, bool> clearedPixels = new Dictionary<Guid, bool>();
        private Coordinates[] currentSelection;
        private Coordinates lastMouseMove;
        private Coordinates lastStartMousePos;
        private Dictionary<Guid, Color[]> startPixelColors;
        private Dictionary<Guid, Thickness> startingOffsets;
        private Coordinates[] startSelection;
        private bool updateViewModelSelection = true;

        public MoveTool()
        {
            ActionDisplay = "Hold mouse to move selected pixels. Hold Ctrl to move all layers.";
            Tooltip = "Moves selected pixels (V). Hold Ctrl to move all layers.";
            Cursor = Cursors.Arrow;
            HideHighlight = true;
            RequiresPreviewLayer = true;
            UseDefaultUndoMethod = true;
        }

        public bool MoveAll { get; set; } = false;

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

        public override void AfterAddedUndo(UndoManager undoManager)
        {
            if (currentSelection != null && currentSelection.Length > 0)
            {
                Change changes = undoManager.UndoStack.Peek();

                // Inject to default undo system change custom changes made by this tool
                foreach (var item in startPixelColors)
                {
                    BitmapPixelChanges beforeMovePixels = BitmapPixelChanges.FromArrays(startSelection, item.Value);
                    Guid layerGuid = item.Key;
                    var oldValue = (LayerChange[])changes.OldValue;

                    if (oldValue.Any(x => x.LayerGuid == layerGuid))
                    {
                        oldValue.First(x => x.LayerGuid == layerGuid).PixelChanges.ChangedPixels
                            .AddRangeOverride(beforeMovePixels.ChangedPixels);

                        ((LayerChange[])changes.NewValue).First(x => x.LayerGuid == layerGuid).PixelChanges.ChangedPixels
                            .AddRangeNewOnly(BitmapPixelChanges
                                .FromSingleColoredArray(startSelection, System.Windows.Media.Colors.Transparent)
                                .ChangedPixels);
                    }
                }
            }
        }

        // This adds undo if there is no selection, reason why this isn't in AfterUndoAdded,
        // is because it doesn't fire if no pixel changes were made.
        public override void OnStoppedRecordingMouseUp(MouseEventArgs e)
        {
            if (currentSelection != null && currentSelection.Length == 0)
            {
                ViewModelMain.Current.BitmapManager.ActiveDocument.UndoManager.AddUndoChange(new Change(
                    ApplyOffsets,
                    new object[] { startingOffsets },
                    ApplyOffsets,
                    new object[] { GetOffsets(affectedLayers) },
                    "Move layers"));
            }
        }

        public override void OnStart(Coordinates startPos)
        {
            ResetSelectionValues(startPos);

            // Move offset if no selection
            Selection selection = ViewModelMain.Current.BitmapManager.ActiveDocument.ActiveSelection;
            if (selection != null && selection.SelectedPoints.Count > 0)
            {
                currentSelection = selection.SelectedPoints.ToArray();
            }
            else
            {
                currentSelection = Array.Empty<Coordinates>();
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || MoveAll)
            {
                affectedLayers = ViewModelMain.Current.BitmapManager.ActiveDocument.Layers.Where(x => x.IsVisible)
                    .ToArray();
            }
            else
            {
                affectedLayers = ViewModelMain.Current.BitmapManager.ActiveDocument
                    .Layers.Where(x => x.IsActive && x.IsVisible).ToArray();
            }

            startSelection = currentSelection;
            startPixelColors = BitmapUtils.GetPixelsForSelection(affectedLayers, startSelection);
            startingOffsets = GetOffsets(affectedLayers);
        }

        public override LayerChange[] Use(Layer layer, Coordinates[] mouseMove, Color color)
        {
            LayerChange[] result = new LayerChange[affectedLayers.Length];
            var end = mouseMove[0];
            for (int i = 0; i < affectedLayers.Length; i++)
            {
                if (currentSelection.Length > 0)
                {
                    var changes = MoveSelection(affectedLayers[i], mouseMove);
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

            return result;
        }

        public BitmapPixelChanges MoveSelection(Layer layer, Coordinates[] mouseMove)
        {
            Coordinates end = mouseMove[0];

            currentSelection = TranslateSelection(end, out Coordinates[] previousSelection);
            if (updateViewModelSelection)
            {
                ViewModelMain.Current.BitmapManager.ActiveDocument.ActiveSelection.SetSelection(currentSelection, SelectionType.New);
            }

            ClearSelectedPixels(layer, previousSelection);

            lastMouseMove = end;
            return BitmapPixelChanges.FromArrays(currentSelection, startPixelColors[layer.LayerGuid]);
        }

        private void ApplyOffsets(object[] parameters)
        {
            Dictionary<Guid, Thickness> offsets = (Dictionary<Guid, Thickness>)parameters[0];
            foreach (var offset in offsets)
            {
                Layer layer = ViewModelMain.Current?.BitmapManager?.
                    ActiveDocument?.Layers?.First(x => x.LayerGuid == offset.Key);
                layer.Offset = offset.Value;
            }
        }

        private Dictionary<Guid, Thickness> GetOffsets(Layer[] layers)
        {
            Dictionary<Guid, Thickness> dict = new Dictionary<Guid, Thickness>();
            for (int i = 0; i < layers.Length; i++)
            {
                dict.Add(layers[i].LayerGuid, layers[i].Offset);
            }

            return dict;
        }

        private BitmapPixelChanges RemoveTransparentPixels(BitmapPixelChanges pixels)
        {
            foreach (var item in pixels.ChangedPixels.Where(x => x.Value.A == 0).ToList())
            {
                pixels.ChangedPixels.Remove(item.Key);
            }

            return pixels;
        }

        private void ResetSelectionValues(Coordinates start)
        {
            lastStartMousePos = start;
            lastMouseMove = start;
            clearedPixels = new Dictionary<Guid, bool>();
            updateViewModelSelection = true;
            startPixelColors = null;
            startSelection = null;
        }

        private Coordinates[] TranslateSelection(Coordinates end, out Coordinates[] previousSelection)
        {
            Coordinates translation = Transform.GetTranslation(lastMouseMove, end);
            previousSelection = currentSelection.ToArray();
            return Transform.Translate(previousSelection, translation);
        }

        private void ClearSelectedPixels(Layer layer, Coordinates[] selection)
        {
            Guid layerGuid = layer.LayerGuid;
            if (!clearedPixels.ContainsKey(layerGuid) || clearedPixels[layerGuid] == false)
            {
                ViewModelMain.Current.BitmapManager.ActiveDocument.Layers.First(x => x == layer)
                    .SetPixels(BitmapPixelChanges.FromSingleColoredArray(selection, System.Windows.Media.Colors.Transparent));

                clearedPixels[layerGuid] = true;
            }
        }
    }
}