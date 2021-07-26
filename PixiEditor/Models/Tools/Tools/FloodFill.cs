using System.Collections.Generic;
using System.Windows.Media;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;

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

            // Find Left Edge of Color Area
            int lFillLoc = coords.X; //the location to check/fill on the left
            int pixelIndex = (coords.X * width) + coords.Y;

            while (true)
            {
                int x = pixelIndex % width;
                int y = pixelIndex / width;
                // fill with the color
                changedCoords.Add(new Coordinates(x, y));
                //**indicate that this pixel has already been checked and filled
                visited[pixelIndex] = true;

                //**de-increment
                lFillLoc--;     //de-increment counter
                pixelIndex--;        //de-increment pixel index
                //**exit loop if we're at edge of bitmap or color area
                if (lFillLoc <= 0 || visited[pixelIndex] || layer.GetPixelWithOffset(x, y) != colorToReplace)
                    break;

            }

            lFillLoc++;

            //FIND RIGHT EDGE OF COLOR AREA
            int rFillLoc = coords.X; //the location to check/fill on the left
            pixelIndex = (width * coords.Y) + coords.X;
            while (true)
            {
                int x = pixelIndex % width;
                int y = pixelIndex / width;
                changedCoords.Add(new Coordinates(x, y));

                visited[pixelIndex] = true;
                rFillLoc++;          // increment counter
                pixelIndex++;
                if (rFillLoc >= width || layer.GetPixelWithOffset(x, y) != colorToReplace || visited[pixelIndex])
                    break;

            }
            rFillLoc--;

            FloodFillRange range = new FloodFillRange(lFillLoc, rFillLoc, coords.Y);
            floodFillQueue.Enqueue(range);
        }

        private void PerformFloodFIll(ref List<Coordinates> changedCords, ref Queue<FloodFillRange> floodFillQueue, ref Layer layer, ref Color colorToReplace, int width, int height, ref bool[] pixelsVisited)
        {
            while (floodFillQueue.Count > 0)
            {
                FloodFillRange range = floodFillQueue.Dequeue();

                //START THE LOOP UPWARDS AND DOWNWARDS
                int upY = range.Y - 1;//so we can pass the y coord by ref
                int downY = range.Y + 1;
                int downPixelxIndex = (width * (range.Y + 1)) + range.StartX;//CoordsToPixelIndex(range.StartX,range.Y+1);
                int upPixelIndex = (width * (range.Y - 1)) + range.StartX;//CoordsToPixelIndex(range.StartX, range.Y - 1);
                for (int i = range.StartX; i <= range.EndX; i++)
                {
                    //START LOOP UPWARDS
                    //if we're not above the top of the bitmap and the pixel above this one is within the color tolerance
                    if (range.Y > 0 && layer.GetPixelWithOffset(i, upY) == colorToReplace && (!pixelsVisited[upPixelIndex]))
                        PerformLinearFill(ref changedCords, ref floodFillQueue,ref layer, new Coordinates(i, upY), width, ref colorToReplace, ref pixelsVisited);
                    //START LOOP DOWNWARDS
                    if (range.Y < (height - 1) && layer.GetPixelWithOffset(i, downY) == colorToReplace && (!pixelsVisited[downPixelxIndex]))
                        PerformLinearFill(ref changedCords, ref floodFillQueue, ref layer, new Coordinates(i, downY), width, ref colorToReplace, ref pixelsVisited);
                    downPixelxIndex++;
                    upPixelIndex++;
                }
            }
        }
    }
}