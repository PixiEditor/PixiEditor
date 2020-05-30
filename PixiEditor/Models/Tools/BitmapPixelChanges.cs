using PixiEditor.Exceptions;
using PixiEditor.Models.Position;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace PixiEditor.Models.Tools
{
    public struct BitmapPixelChanges
    {
        public static BitmapPixelChanges Empty = new BitmapPixelChanges(new Dictionary<Coordinates, Color>());
        public Dictionary<Coordinates, Color> ChangedPixels { get; set; }

        public BitmapPixelChanges(Dictionary<Coordinates, Color> changedPixels)
        {
            ChangedPixels = changedPixels;
        }
        /// <summary>
        /// Builds BitmapPixelChanges with only one color for specified coordinates
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="color"></param>
        /// <returns>Single-colored BitmapPixelChanges</returns>
        public static BitmapPixelChanges FromSingleColoredArray(Coordinates[] coordinates, Color color)
        {
            Dictionary<Coordinates, Color> dict = new Dictionary<Coordinates, Color>();
            for (int i = 0; i < coordinates.Length; i++)
            {
                dict.Add(coordinates[i], color);
            }
            return new BitmapPixelChanges(dict);
        }

        /// <summary>
        /// Builds BitmapPixelChanges using 2 same-length enumerables of coordinates and colors
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static BitmapPixelChanges FromArrays(IEnumerable<Coordinates> coordinates, IEnumerable<Color> color)
        {
            var coordinateArray = coordinates.ToArray();
            var colorArray = color.ToArray();
            if (coordinateArray.Length != colorArray.Length)
            {
                throw new ArrayLengthMismatchException();
            }
            Dictionary<Coordinates, Color> dict = new Dictionary<Coordinates, Color>();
            for (int i = 0; i < coordinateArray.Length; i++)
            {
                dict.Add(coordinateArray[i], colorArray[i]);
            }
            return new BitmapPixelChanges(dict);
        }
    }
}
