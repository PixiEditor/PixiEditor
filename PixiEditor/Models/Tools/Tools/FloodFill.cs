using System.Collections.Generic;
using System.Windows.Media;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.ViewModels;

namespace PixiEditor.Models.Tools.Tools
{
    public class FloodFill : BitmapOperationTool
    {
        public FloodFill()
        {
            Tooltip = "Fills area with color (G)";
        }

        public override ToolType ToolType => ToolType.Bucket;

        public override LayerChange[] Use(Layer layer, Coordinates[] coordinates, Color color)
        {
            return Only(ForestFire(layer, coordinates[0], color), layer);
        }

        private BitmapPixelChanges ForestFire(Layer layer, Coordinates startingCoords, Color newColor)
        {
            List<Coordinates> changedCoords = new List<Coordinates>();

            int width = ViewModelMain.Current.BitmapManager.ActiveDocument.Width;
            int height = ViewModelMain.Current.BitmapManager.ActiveDocument.Height;

            var visited = new bool[width, height];

            Color colorToReplace = layer.GetPixelWithOffset(startingCoords.X, startingCoords.Y);

            var stack = new Stack<Coordinates>();
            stack.Push(new Coordinates(startingCoords.X, startingCoords.Y));

            while (stack.Count > 0)
            {
                var cords = stack.Pop();
                var relativeCords = layer.GetRelativePosition(cords);

                if (cords.X < 0 || cords.X > width - 1)
                {
                    continue;
                }

                if (cords.Y < 0 || cords.Y > height - 1)
                {
                    continue;
                }

                if (visited[cords.X, cords.Y])
                {
                    continue;
                }

                if (layer.GetPixel(relativeCords.X, relativeCords.Y) == newColor)
                {
                    continue;
                }

                if (layer.GetPixel(relativeCords.X, relativeCords.Y) == colorToReplace)
                {
                    changedCoords.Add(new Coordinates(cords.X, cords.Y));
                    visited[cords.X, cords.Y] = true;
                    stack.Push(new Coordinates(cords.X, cords.Y - 1));
                    stack.Push(new Coordinates(cords.X, cords.Y + 1));
                    stack.Push(new Coordinates(cords.X - 1, cords.Y));
                    stack.Push(new Coordinates(cords.X + 1, cords.Y));
                }
            }

            return BitmapPixelChanges.FromSingleColoredArray(changedCoords, newColor);
        }
    }
}