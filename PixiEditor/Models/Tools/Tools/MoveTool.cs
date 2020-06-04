using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Images;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.Tools.Tools
{
    public class MoveTool : BitmapOperationTool
    {
        public override ToolType ToolType => ToolType.Move;
        private Coordinates[] _startSelection;
        private Coordinates _lastStartMousePos;
        private Coordinates _lastMouseMove;
        private Dictionary<Layer, Color[]> _startPixelColors;
        private Dictionary<Layer, bool> _clearedPixels = new Dictionary<Layer, bool>();
        private Coordinates[] _currentSelection;
        private bool _updateViewModelSelection = true;
        private BitmapPixelChanges _clearedPixelsChange;
        private Layer[] _affectedLayers;

        public MoveTool()
        {
            Tooltip = "Moves selected pixels. (V)";
            Cursor = Cursors.Arrow;
            HideHighlight = true;
            RequiresPreviewLayer = true;
            UseDefaultUndoMethod = true;
        }

        public override void AfterAddedUndo()
        {
            //Inject to default undo system change custom changes made by this tool
            foreach(var item in _startPixelColors)
            {
                BitmapPixelChanges beforeMovePixels = BitmapPixelChanges.
                    FromArrays(_startSelection, item.Value);
                Change changes = UndoManager.UndoStack.Peek();
                int layerIndex = ViewModelMain.Current.BitmapManager.ActiveDocument.Layers.IndexOf(item.Key);

                (changes.OldValue as LayerChange[])[layerIndex].PixelChanges.ChangedPixels.
                    AddRangeOverride(beforeMovePixels.ChangedPixels);

                (changes.NewValue as LayerChange[])[layerIndex].PixelChanges.ChangedPixels.
                    AddRangeOverride(_clearedPixelsChange.ChangedPixels);
            }     
        }

        public override LayerChange[] Use(Layer layer, Coordinates[] mouseMove, Color color)
        {
            Coordinates start = mouseMove[^1];
            if (_lastStartMousePos != start) //I am aware that this could be moved to OnMouseDown, but it is executed before Use, so I didn't want to complicate for now
            {
                ResetSelectionValues(start);
                if (ViewModelMain.Current.ActiveSelection.SelectedPoints.Count == 0) //Move every pixel if none is selected
                {
                    SelectTool select = new SelectTool();
                    _currentSelection = select.GetAllSelection();
                    _updateViewModelSelection = false;
                }
                else
                {
                    _currentSelection = ViewModelMain.Current.ActiveSelection.SelectedPoints.ToArray();
                }

                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    _affectedLayers = ViewModelMain.Current.BitmapManager.ActiveDocument.Layers.Where(x=> x.IsVisible).ToArray();
                }
                else
                {
                    _affectedLayers = new Layer[] { layer };
                }

                _startSelection = _currentSelection;
                _startPixelColors = BitmapUtils.GetPixelsForSelection(_affectedLayers, _startSelection);
            }

            LayerChange[] result = new LayerChange[_affectedLayers.Length];

            for (int i = 0; i < _affectedLayers.Length; i++)
            {
                var changes = MoveSelection(_affectedLayers[i], mouseMove);
                changes = RemoveTransparentPixels(changes);
                
                result[i] = new LayerChange(changes, _affectedLayers[i]);
            }
            return result;
        }

        private BitmapPixelChanges RemoveTransparentPixels(BitmapPixelChanges pixels)
        {
            foreach (var item in pixels.ChangedPixels.Where(x => x.Value.A == 0).ToList())
            {
                pixels.ChangedPixels.Remove(item.Key);
            }
            return pixels;
        }

        public BitmapPixelChanges MoveSelection(Layer layer, Coordinates[] mouseMove)
        {
            Coordinates end = mouseMove[0];

            _currentSelection = TranslateSelection(end, out Coordinates[] previousSelection);
            if (_updateViewModelSelection)
            {
                ViewModelMain.Current.ActiveSelection.SetSelection(_currentSelection, Enums.SelectionType.New);
            }
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
        }

        private Coordinates[] TranslateSelection(Coordinates end, out Coordinates[] previousSelection)
        {
            Coordinates translation = ImageManipulation.Transform.GetTranslation(_lastMouseMove, end);
            previousSelection = _currentSelection.ToArray();
            return ImageManipulation.Transform.Translate(previousSelection, translation);
        }

        private void ClearSelectedPixels(Layer layer, Coordinates[] selection)
        {
            if (!_clearedPixels.ContainsKey(layer) || _clearedPixels[layer] == false)
            {
                _clearedPixelsChange = BitmapPixelChanges.FromSingleColoredArray(selection, System.Windows.Media.Colors.Transparent);
                ViewModelMain.Current.BitmapManager.ActiveDocument.Layers.First(x=> x == layer).ApplyPixels(_clearedPixelsChange);

                _clearedPixels[layer] = true;
            }
        }       
    }
}
