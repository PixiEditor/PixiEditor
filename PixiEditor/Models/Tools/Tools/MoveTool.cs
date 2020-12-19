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
using PixiEditor.ViewModels;
using Transform = PixiEditor.Models.ImageManipulation.Transform;

namespace PixiEditor.Models.Tools.Tools
{
    public class MoveTool : BitmapOperationTool
    {
        private Layer[] affectedLayers;
        private Dictionary<Layer, bool> clearedPixels = new Dictionary<Layer, bool>();
        private Coordinates[] currentSelection;
        private Coordinates lastMouseMove;
        private Coordinates lastStartMousePos;
        private Dictionary<Layer, Color[]> startPixelColors;
        private Dictionary<Layer, Thickness> startingOffsets;
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

        public override ToolType ToolType => ToolType.Move;

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
            if (currentSelection != null && currentSelection.Length != 0)
            {
                // Inject to default undo system change custom changes made by this tool
                foreach (var item in startPixelColors)
                {
                    BitmapPixelChanges beforeMovePixels = BitmapPixelChanges.FromArrays(startSelection, item.Value);
                    Change changes = undoManager.UndoStack.Peek();
                    int layerIndex = ViewModelMain.Current.BitmapManager.ActiveDocument.Layers.IndexOf(item.Key);

                    ((LayerChange[])changes.OldValue).First(x => x.LayerIndex == layerIndex).PixelChanges.ChangedPixels
                        .AddRangeOverride(beforeMovePixels.ChangedPixels);

                    ((LayerChange[])changes.NewValue).First(x => x.LayerIndex == layerIndex).PixelChanges.ChangedPixels
                        .AddRangeNewOnly(BitmapPixelChanges
                            .FromSingleColoredArray(startSelection, System.Windows.Media.Colors.Transparent)
                            .ChangedPixels);
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

        public override LayerChange[] Use(Layer layer, Coordinates[] mouseMove, Color color)
        {
            Coordinates start = mouseMove[^1];

            // I am aware that this could be moved to OnMouseDown, but it is executed before Use, so I didn't want to complicate for now
            if (lastStartMousePos != start)
            {
                ResetSelectionValues(start);

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
                    affectedLayers = new[] { layer };
                }

                startSelection = currentSelection;
                startPixelColors = BitmapUtils.GetPixelsForSelection(affectedLayers, startSelection);
                startingOffsets = GetOffsets(affectedLayers);
            }

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
            return BitmapPixelChanges.FromArrays(currentSelection, startPixelColors[layer]);
        }

        private void ApplyOffsets(object[] parameters)
        {
            Dictionary<Layer, Thickness> offsets = (Dictionary<Layer, Thickness>)parameters[0];
            foreach (var offset in offsets)
            {
                offset.Key.Offset = offset.Value;
            }
        }

        private Dictionary<Layer, Thickness> GetOffsets(Layer[] layers)
        {
            Dictionary<Layer, Thickness> dict = new Dictionary<Layer, Thickness>();
            for (int i = 0; i < layers.Length; i++)
            {
                dict.Add(layers[i], layers[i].Offset);
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
            clearedPixels = new Dictionary<Layer, bool>();
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
            if (!clearedPixels.ContainsKey(layer) || clearedPixels[layer] == false)
            {
                ViewModelMain.Current.BitmapManager.ActiveDocument.Layers.First(x => x == layer)
                    .SetPixels(BitmapPixelChanges.FromSingleColoredArray(selection, System.Windows.Media.Colors.Transparent));

                clearedPixels[layer] = true;
            }
        }
    }
}