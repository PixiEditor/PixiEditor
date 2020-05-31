using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.ViewModels;
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
        private Color[] _startPixelColors;
        private bool _clearedPixels = false;
        private Coordinates[] _currentSelection;
        private bool _updateViewModelSelection = true;
        private BitmapPixelChanges _clearedPixelsChange;

        public MoveTool()
        {
            Tooltip = "Moves selected pixels. (M)";
            Cursor = Cursors.Arrow;
            HideHighlight = true;
            RequiresPreviewLayer = true;
            UseDefaultUndoMethod = true;
        }

        public override void AfterAddedUndo()
        {
            //Injecting to default undo system change custom changes made by this tool
            BitmapPixelChanges beforeMovePixels = BitmapPixelChanges.FromArrays(_startSelection, _startPixelColors);
            Change changes = UndoManager.UndoStack.Peek();
            (changes.OldValue as LayerChanges).PixelChanges.ChangedPixels.
                AddRangeNewOnly(beforeMovePixels.ChangedPixels);
            (changes.NewValue as LayerChanges).PixelChanges.ChangedPixels.AddRangeNewOnly(_clearedPixelsChange.ChangedPixels);

        }

        public override BitmapPixelChanges Use(Layer layer, Coordinates[] mouseMove, Color color)
        {
            Coordinates start = mouseMove[^1];
            if (_lastStartMousePos != start) //I am aware that this could be moved to OnMouseDown, but it is executed before Use, so I didn't want to complicate for now
            {
                ResetSelectionValues(start);
                if (ViewModelMain.Current.ActiveSelection.SelectedPoints == null) //Move every pixel if none is selected
                {
                    SelectTool select = new SelectTool();
                    _currentSelection = select.GetAllSelection();
                    _updateViewModelSelection = false;
                }
                else
                {
                    _currentSelection = ViewModelMain.Current.ActiveSelection.SelectedPoints;
                }
                _startSelection = _currentSelection;
                _startPixelColors = GetPixelsForSelection(layer, _startSelection);
            }

            return MoveSelection(layer, mouseMove);
        }

        public BitmapPixelChanges MoveSelection(Layer layer, Coordinates[] mouseMove)
        {
            Coordinates end = mouseMove[0];

            _currentSelection = TranslateSelection(end, out Coordinates[] previousSelection);
            if (_updateViewModelSelection)
            {
                ViewModelMain.Current.ActiveSelection = new Selection(_currentSelection);
            }
            ClearSelectedPixels(layer, previousSelection);


            _lastMouseMove = end;
            return BitmapPixelChanges.FromArrays(
                        _currentSelection, _startPixelColors);
        }

        private void ResetSelectionValues(Coordinates start)
        {
            _lastStartMousePos = start;
            _lastMouseMove = start;
            _clearedPixels = false;
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
            if (_clearedPixels == false)
            {
                _clearedPixelsChange = BitmapPixelChanges.FromSingleColoredArray(selection, System.Windows.Media.Colors.Transparent);
                layer.ApplyPixels(_clearedPixelsChange);

                _clearedPixels = true;
            }
        }

        private Color[] GetPixelsForSelection(Layer layer, Coordinates[] selection)
        {
            Color[] pixels = new Color[_startSelection.Length];
            layer.LayerBitmap.Lock();

            for (int i = 0; i < pixels.Length; i++)
            {
                Coordinates position = selection[i];
                if (position.X < 0 || position.X > layer.Width - 1 || position.Y < 0 || position.Y > layer.Height - 1)
                    continue;
                pixels[i] = layer.LayerBitmap.GetPixel(position.X, position.Y);
            }
            layer.LayerBitmap.Unlock();
            return pixels;
        }
    }
}
