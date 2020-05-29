using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace PixiEditor.Models.Tools.Tools
{
    public class SelectTool : ReadonlyTool
    {
        public override ToolType ToolType => ToolType.Select;

        public override void Use(Coordinates[] pixels)
        {
            Select(pixels);
        }

        private void Select(Coordinates[] pixels)
        {
            Coordinates[] selection = GetRectangleSelectionForPoints(pixels[^1], pixels[0]);
            ViewModelMain.Current.ActiveSelection = new DataHolders.Selection(selection.ToArray());
        }

        public Coordinates[] GetRectangleSelectionForPoints(Coordinates start, Coordinates end)
        {
            RectangleTool rectangleTool = new RectangleTool();
            List<Coordinates> selection = rectangleTool.CreateRectangle(start, end, 1).ToList();
            selection.AddRange(rectangleTool.CalculateFillForRectangle(start, end, 1));
            return selection.ToArray();
        }

        /// <summary>
        /// Gets coordinates of every pixel in root layer
        /// </summary>
        /// <returns>Coordinates array of pixels</returns>
        public Coordinates[] GetAllSelection()
        {

            return GetAllSelection(ViewModelMain.Current.BitmapManager.Layers[0]);
        }

        /// <summary>
        /// Gets coordinates of every pixel in choosen layer
        /// </summary>
        /// <param name="layer"></param>
        /// <returns>Coordinates array of pixels</returns>
        public Coordinates[] GetAllSelection(Layer layer)
        {
            return GetRectangleSelectionForPoints(new Coordinates(0,0), new Coordinates(layer.Width - 1, layer.Height - 1));
        }
    }
}
