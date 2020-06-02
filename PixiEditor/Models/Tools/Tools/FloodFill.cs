using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.Tools.Tools
{
    public class FloodFill : BitmapOperationTool
    {
        public override ToolType ToolType => ToolType.Bucket;

        public FloodFill()
        {
            Tooltip = "Fills area with color (G)";
        }

        public override LayerChange[] Use(Layer layer, Coordinates[] coordinates, Color color)
        {
            return new LayerChange[] { new LayerChange(ForestFire(layer, coordinates[0], color), layer) };
        }

        public BitmapPixelChanges ForestFire(Layer layer, Coordinates startingCoords, Color newColor)
        {
            List<Coordinates> changedCoords = new List<Coordinates>();

            WriteableBitmap bitmap = layer.LayerBitmap.Clone();
            Color colorToReplace = bitmap.GetPixel(startingCoords.X, startingCoords.Y);

            var stack = new Stack<Tuple<int, int>>();
            stack.Push(Tuple.Create(startingCoords.X, startingCoords.Y));

            bitmap.Lock();
            while (stack.Count > 0)
            {
                var point = stack.Pop();
                if (point.Item1 < 0 || point.Item1 > layer.Height - 1) continue;
                if (point.Item2 < 0 || point.Item2 > layer.Width - 1) continue;
                if (bitmap.GetPixel(point.Item1, point.Item2) == newColor) continue;

                if (bitmap.GetPixel(point.Item1, point.Item2) == colorToReplace)
                {
                    changedCoords.Add(new Coordinates(point.Item1, point.Item2));
                    bitmap.SetPixel(point.Item1, point.Item2, newColor);
                    stack.Push(Tuple.Create(point.Item1, point.Item2 - 1));
                    stack.Push(Tuple.Create(point.Item1 + 1, point.Item2));
                    stack.Push(Tuple.Create(point.Item1, point.Item2 + 1));
                    stack.Push(Tuple.Create(point.Item1 - 1, point.Item2));
                }
            }
            bitmap.Unlock();
            return BitmapPixelChanges.FromSingleColoredArray(changedCoords.ToArray(), newColor);
        }
    }
}
