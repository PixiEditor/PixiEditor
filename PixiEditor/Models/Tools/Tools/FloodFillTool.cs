using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using SkiaSharp;
using System.Collections.Generic;
using System.Windows;

namespace PixiEditor.Models.Tools.Tools
{
    internal class FloodFillTool : BitmapOperationTool
    {
        private BitmapManager BitmapManager { get; }
        private SKPaint fillPaint = new SKPaint() { BlendMode = SKBlendMode.Src };

        public FloodFillTool(BitmapManager bitmapManager)
        {
            ActionDisplay = "Press on an area to fill it.";
            BitmapManager = bitmapManager;
        }

        public override string Tooltip => "Fills area with color. (G)";

        public override void Use(Layer activeLayer, Layer previewLayer, IEnumerable<Layer> allLayers, IReadOnlyList<Coordinates> recordedMouseMovement, SKColor color)
        {
            if (activeLayer.IsReset)
            {
                activeLayer.DynamicResizeAbsolute(new(0, 0, BitmapManager.ActiveDocument.Width, BitmapManager.ActiveDocument.Height));
                activeLayer.LayerBitmap.SkiaSurface.Canvas.Clear(color);
                activeLayer.InvokeLayerBitmapChange();
            }
            else
            {
                LinearFill(activeLayer, recordedMouseMovement[^1], color);
            }
        }

        public void LinearFill(Layer layer, Coordinates startingCoords, SKColor newColor)
        {
            Queue<FloodFillRange> floodFillQueue = new Queue<FloodFillRange>();
            SKColor colorToReplace = layer.GetPixelWithOffset(startingCoords.X, startingCoords.Y);
            if ((colorToReplace.Alpha == 0 && newColor.Alpha == 0) ||
                colorToReplace == newColor)
                return;

            int width = BitmapManager.ActiveDocument.Width;
            int height = BitmapManager.ActiveDocument.Height;
            if (startingCoords.X < 0 || startingCoords.Y < 0 || startingCoords.X >= width || startingCoords.Y >= height)
                return;
            var visited = new bool[width * height];

            fillPaint.Color = newColor;

            Int32Rect dirtyRect = new Int32Rect(startingCoords.X, startingCoords.Y, 1, 1);

            PerformLinearFill(layer, floodFillQueue, startingCoords, width, colorToReplace, ref dirtyRect, visited);
            PerformFloodFIll(layer, floodFillQueue, colorToReplace, ref dirtyRect, width, height, visited);

            layer.InvokeLayerBitmapChange(dirtyRect);
        }

        private void PerformLinearFill(
            Layer layer, Queue<FloodFillRange> floodFillQueue,
            Coordinates coords, int width, SKColor colorToReplace, ref Int32Rect dirtyRect, bool[] visited)
        {
            // Find the Left Edge of the Color Area
            int fillXLeft = coords.X;
            while (true)
            {
                // Indicate that this pixel has been checked
                int pixelIndex = (coords.Y * width) + fillXLeft;
                visited[pixelIndex] = true;

                // Move one pixel to the left
                fillXLeft--;
                // Exit the loop if we're at edge of the bitmap or the color area
                if (fillXLeft < 0 || visited[pixelIndex - 1] || layer.GetPixelWithOffset(fillXLeft, coords.Y) != colorToReplace)
                    break;
            }
            int lastCheckedPixelLeft = fillXLeft + 1;

            // Find the Right Edge of the Color Area
            int fillXRight = coords.X;
            while (true)
            {
                int pixelIndex = (coords.Y * width) + fillXRight;
                visited[pixelIndex] = true;

                fillXRight++;
                if (fillXRight >= width || visited[pixelIndex + 1] || layer.GetPixelWithOffset(fillXRight, coords.Y) != colorToReplace)
                    break;
            }
            int lastCheckedPixelRight = fillXRight - 1;

            layer.DynamicResizeAbsolute(new(lastCheckedPixelLeft, coords.Y, lastCheckedPixelRight - lastCheckedPixelLeft + 1, 1));
            int relativeY = coords.Y - layer.OffsetY;
            layer.LayerBitmap.SkiaSurface.Canvas.DrawLine(lastCheckedPixelLeft - layer.OffsetX, relativeY, lastCheckedPixelRight - layer.OffsetX + 1, relativeY, fillPaint);
            dirtyRect = dirtyRect.Expand(new Int32Rect(lastCheckedPixelLeft, coords.Y, lastCheckedPixelRight - lastCheckedPixelLeft + 1, 1));

            FloodFillRange range = new FloodFillRange(lastCheckedPixelLeft, lastCheckedPixelRight, coords.Y);
            floodFillQueue.Enqueue(range);
        }

        private void PerformFloodFIll(
            Layer layer, Queue<FloodFillRange> floodFillQueue,
            SKColor colorToReplace, ref Int32Rect dirtyRect, int width, int height, bool[] pixelsVisited)
        {
            while (floodFillQueue.Count > 0)
            {
                FloodFillRange range = floodFillQueue.Dequeue();

                //START THE LOOP UPWARDS AND DOWNWARDS
                int upY = range.Y - 1; //so we can pass the y coord by ref
                int downY = range.Y + 1;
                int downPixelxIndex = (width * (range.Y + 1)) + range.StartX;
                int upPixelIndex = (width * (range.Y - 1)) + range.StartX;
                for (int i = range.StartX; i <= range.EndX; i++)
                {
                    //START LOOP UPWARDS
                    //if we're not above the top of the bitmap and the pixel above this one is within the color tolerance
                    if (range.Y > 0 && (!pixelsVisited[upPixelIndex]) && layer.GetPixelWithOffset(i, upY) == colorToReplace)
                        PerformLinearFill(layer, floodFillQueue, new Coordinates(i, upY), width, colorToReplace, ref dirtyRect, pixelsVisited);
                    //START LOOP DOWNWARDS
                    if (range.Y < (height - 1) && (!pixelsVisited[downPixelxIndex]) && layer.GetPixelWithOffset(i, downY) == colorToReplace)
                        PerformLinearFill(layer, floodFillQueue, new Coordinates(i, downY), width, colorToReplace, ref dirtyRect, pixelsVisited);
                    downPixelxIndex++;
                    upPixelIndex++;
                }
            }
        }
    }
}
