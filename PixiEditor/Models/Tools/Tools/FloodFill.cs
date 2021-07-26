using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using System.Collections.Generic;
using System.Windows.Media;

namespace PixiEditor.Models.Tools.Tools
{
    public class FloodFill : BitmapOperationTool
    {
        private BitmapManager BitmapManager { get; }


        public FloodFill(BitmapManager bitmapManager)
        {
            ActionDisplay = "Press on a area to fill it.";
            BitmapManager = bitmapManager;
        }

        public override string Tooltip => "Fills area with color. (G)";

        public override LayerChange[] Use(Layer layer, List<Coordinates> coordinates, Color color)
        {
            return Only(LinearFill(layer, coordinates[0], color), layer);
        }

        public BitmapPixelChanges LinearFill(Layer layer, Coordinates startingCoords, Color newColor)
        {
            List<Coordinates> changedCoords = new List<Coordinates>();
            Queue<FloodFillRange> floodFillQueue = new Queue<FloodFillRange>();
            Color colorToReplace = layer.GetPixelWithOffset(startingCoords.X, startingCoords.Y);
            int width = BitmapManager.ActiveDocument.Width;
            int height = BitmapManager.ActiveDocument.Height;
            var visited = new bool[width * height];


            PerformLinearFill(ref changedCoords, ref floodFillQueue, ref layer, startingCoords, width, ref colorToReplace, ref visited);
            PerformFloodFIll(ref changedCoords, ref floodFillQueue, ref layer, ref colorToReplace, width, height, ref visited);

            return BitmapPixelChanges.FromSingleColoredArray(changedCoords, newColor);
        }

        private void PerformLinearFill(ref List<Coordinates> changedCoords, ref Queue<FloodFillRange> floodFillQueue, ref Layer layer, Coordinates coords, int width, ref Color colorToReplace, ref bool[] visited)
        {
            // Find the Left Edge of the Color Area
            int fillXLeft = coords.X;
            while (true)
            {
                // Fill with the color
                changedCoords.Add(new Coordinates(fillXLeft, coords.Y));

                // Indicate that this pixel has already been checked and filled
                int pixelIndex = (coords.Y * width) + fillXLeft;
                visited[pixelIndex] = true;

                // Move one pixel to the left
                fillXLeft--;
                // Exit the loop if we're at edge of the bitmap or the color area
                if (fillXLeft < 0 || visited[pixelIndex - 1] || layer.GetPixelWithOffset(fillXLeft, coords.Y) != colorToReplace)
                    break;
            }
            int lastFilledPixelLeft = fillXLeft + 1;


            // Find the Right Edge of the Color Area
            int fillXRight = coords.X;
            while (true)
            {
                changedCoords.Add(new Coordinates(fillXRight, coords.Y));

                int pixelIndex = (coords.Y * width) + fillXRight;
                visited[pixelIndex] = true;

                fillXRight++;
                if (fillXRight >= width || visited[pixelIndex + 1] || layer.GetPixelWithOffset(fillXRight, coords.Y) != colorToReplace)
                    break;
            }
            int lastFilledPixelRight = fillXRight - 1;


            FloodFillRange range = new FloodFillRange(lastFilledPixelLeft, lastFilledPixelRight, coords.Y);
            floodFillQueue.Enqueue(range);
        }

        private void PerformFloodFIll(ref List<Coordinates> changedCords, ref Queue<FloodFillRange> floodFillQueue, ref Layer layer, ref Color colorToReplace, int width, int height, ref bool[] pixelsVisited)
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
                        PerformLinearFill(ref changedCords, ref floodFillQueue, ref layer, new Coordinates(i, upY), width, ref colorToReplace, ref pixelsVisited);
                    //START LOOP DOWNWARDS
                    if (range.Y < (height - 1) && (!pixelsVisited[downPixelxIndex]) && layer.GetPixelWithOffset(i, downY) == colorToReplace)
                        PerformLinearFill(ref changedCords, ref floodFillQueue, ref layer, new Coordinates(i, downY), width, ref colorToReplace, ref pixelsVisited);
                    downPixelxIndex++;
                    upPixelIndex++;
                }
            }
        }
    }
}
