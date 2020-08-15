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
        public bool MoveAll { get; set; } = false;

        public override ToolType ToolType => ToolType.Move;
        private Layer[] _affectedLayers;
        private Dictionary<Layer, bool> _clearedPixels = new Dictionary<Layer, bool>();
        private Coordinates[] _currentSelection;
        private Coordinates _lastMouseMove;
        private Coordinates _lastStartMousePos;
        private Dictionary<Layer, Color[]> _startPixelColors;
        private Dictionary<Layer, Thickness> _startingOffsets;
        private Coordinates[] _startSelection;
        private bool _updateViewModelSelection = true;

        public MoveTool()
        {
            Tooltip = "Moves selected pixels (V). Hold Ctrl to move all layers";
            Cursor = Cursors.Arrow;
            HideHighlight = true;
            RequiresPreviewLayer = true;
            UseDefaultUndoMethod = true;
        }

        public override void AfterAddedUndo()
        {
            if (_currentSelection != null && _currentSelection.Length != 0)
            {
                //Inject to default undo system change custom changes made by this tool
                foreach (var item in _startPixelColors)
                {
                    BitmapPixelChanges beforeMovePixels = BitmapPixelChanges.FromArrays(_startSelection, item.Value);
                    Change changes = UndoManager.UndoStack.Peek();
                    int layerIndex = ViewModelMain.Current.BitmapManager.ActiveDocument.Layers.IndexOf(item.Key);

                    ((LayerChange[]) changes.OldValue).First(x=> x.LayerIndex == layerIndex).PixelChanges.ChangedPixels
                        .AddRangeOverride(beforeMovePixels.ChangedPixels);

                    ((LayerChange[]) changes.NewValue).First(x => x.LayerIndex == layerIndex).PixelChanges.ChangedPixels
                        .AddRangeNewOnly(BitmapPixelChanges
                            .FromSingleColoredArray(_startSelection, System.Windows.Media.Colors.Transparent)
                            .ChangedPixels);
                }
            }
        }

        public override void OnMouseUp(MouseEventArgs e) //This adds undo if there is no selection, reason why this isn't in AfterUndoAdded,
        {   //is because it doesn't fire if no pixel changes were made.
            if (_currentSelection != null && _currentSelection.Length == 0)
            {
                UndoManager.AddUndoChange(new Change(ApplyOffsets, new object[]{_startingOffsets}, 
                    ApplyOffsets, new object[] { GetOffsets(_affectedLayers)}, "Move layers"));
            }
        }

        private void ApplyOffsets(object[] parameters)
        {
            Dictionary<Layer, Thickness> offsets = (Dictionary<Layer, Thickness>)parameters[0];
            foreach (var offset in offsets)
            {
                offset.Key.Offset = offset.Value;
            }
        }

        public override LayerChange[] Use(Layer layer, Coordinates[] mouseMove, Color color)
        {
            Coordinates start = mouseMove[^1];
            if (_lastStartMousePos != start) //I am aware that this could be moved to OnMouseDown, but it is executed before Use, so I didn't want to complicate for now
            {
                ResetSelectionValues(start);
                if (ViewModelMain.Current.ActiveSelection != null && ViewModelMain.Current.ActiveSelection.SelectedPoints.Count > 0) //Move offset if no selection
                {
                    _currentSelection = ViewModelMain.Current.ActiveSelection.SelectedPoints.ToArray();
                }
                else
                {
                    _currentSelection = Array.Empty<Coordinates>();
                }

                if (Keyboard.IsKeyDown(Key.LeftCtrl) || MoveAll)
                    _affectedLayers = ViewModelMain.Current.BitmapManager.ActiveDocument.Layers.Where(x => x.IsVisible)
                        .ToArray();
                else
                    _affectedLayers = new[] {layer};

                _startSelection = _currentSelection;
                _startPixelColors = BitmapUtils.GetPixelsForSelection(_affectedLayers, _startSelection);
                _startingOffsets = GetOffsets(_affectedLayers);
            }

            LayerChange[] result = new LayerChange[_affectedLayers.Length];
            var end = mouseMove[0];
            for (int i = 0; i < _affectedLayers.Length; i++)
            {
                if (_currentSelection.Length > 0)
                {
                    var changes = MoveSelection(_affectedLayers[i], mouseMove);
                    changes = RemoveTransparentPixels(changes);

                    result[i] = new LayerChange(changes, _affectedLayers[i]);
                }
                else
                {
                    var vector = Transform.GetTranslation(_lastMouseMove, end);
                    _affectedLayers[i].Offset = new Thickness(_affectedLayers[i].OffsetX + vector.X, _affectedLayers[i].OffsetY + vector.Y, 0, 0);
                    result[i] = new LayerChange(BitmapPixelChanges.Empty, _affectedLayers[i]);
                }
            }
            _lastMouseMove = end;

            return result;
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
                pixels.ChangedPixels.Remove(item.Key);
            return pixels;
        }

        public BitmapPixelChanges MoveSelection(Layer layer, Coordinates[] mouseMove)
        {
            Coordinates end = mouseMove[0];

            _currentSelection = TranslateSelection(end, out Coordinates[] previousSelection);
            if (_updateViewModelSelection)
                ViewModelMain.Current.ActiveSelection.SetSelection(_currentSelection, SelectionType.New);
            ClearSelectedPixels(layer, previousSelection);


            _lastMouseMove = end;
            return BitmapPixelChanges.FromArrays(_currentSelection, _startPixelColors[layer]);
        }

        private void ResetSelectionValues(Coordinates start)
        {
            _lastStartMousePos = start;
            _lastMouseMove = start;
            _clearedPixels = new Dictionary<Layer, bool>();
            _updateViewModelSelection = true;
            _startPixelColors = null;
            _startSelection = null;
        }

        private Coordinates[] TranslateSelection(Coordinates end, out Coordinates[] previousSelection)
        {
            Coordinates translation = Transform.GetTranslation(_lastMouseMove, end);
            previousSelection = _currentSelection.ToArray();
            return Transform.Translate(previousSelection, translation);
        }

        private void ClearSelectedPixels(Layer layer, Coordinates[] selection)
        {
            if (!_clearedPixels.ContainsKey(layer) || _clearedPixels[layer] == false)
            {
                ViewModelMain.Current.BitmapManager.ActiveDocument.Layers.First(x => x == layer)
                    .SetPixels(BitmapPixelChanges.FromSingleColoredArray(selection, System.Windows.Media.Colors.Transparent));

                _clearedPixels[layer] = true;
            }
        }
    }
}