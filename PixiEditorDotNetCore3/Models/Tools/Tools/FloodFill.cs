using PixiEditorDotNetCore3.Models.Layers;
using PixiEditorDotNetCore3.Models.Position;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditorDotNetCore3.Models.Tools.Tools
{
    public class FloodFill : Tool
    {
        public override ToolType ToolType => ToolType.Bucket;

        public FloodFill()
        {
            ExecutesItself = true;
        }

        public override BitmapPixelChanges Use(Layer layer, Coordinates startingCoords, Color color, int toolSize)
        {
            return ForestFire(layer, startingCoords, color);
        }

        public BitmapPixelChanges ForestFire(Layer layer, Coordinates startingCoords, Color newColor)
        {
            List<Coordinates> changedCoords = new List<Coordinates>();

            Color colorToReplace = layer.LayerBitmap.GetPixel(startingCoords.X, startingCoords.Y);

            var stack = new Stack<Tuple<int, int>>();
            stack.Push(Tuple.Create(startingCoords.X, startingCoords.Y));

            while (stack.Count > 0)
            {
                var point = stack.Pop();
                if (point.Item1 < 0 || point.Item1 > layer.Height - 1) continue;
                if (point.Item2 < 0 || point.Item2 > layer.Width - 1) continue;
                if (layer.LayerBitmap.GetPixel(point.Item1, point.Item2) == newColor) continue;

                if (layer.LayerBitmap.GetPixel(point.Item1, point.Item2) == colorToReplace)
                {
                    changedCoords.Add(new Coordinates(point.Item1, point.Item2));
                    layer.LayerBitmap.SetPixel(point.Item1, point.Item2, newColor);
                    stack.Push(Tuple.Create(point.Item1, point.Item2 - 1));
                    stack.Push(Tuple.Create(point.Item1 + 1, point.Item2));
                    stack.Push(Tuple.Create(point.Item1, point.Item2 + 1));
                    stack.Push(Tuple.Create(point.Item1 - 1, point.Item2));
                }
            }
            return new BitmapPixelChanges(changedCoords.ToArray(), newColor);
        }
    }
}
