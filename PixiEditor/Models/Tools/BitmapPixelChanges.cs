using PixiEditor.Exceptions;
using PixiEditor.Models.Position;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace PixiEditor.Models.Tools
{
    public struct BitmapPixelChanges
    {
        public Dictionary<Coordinates, Color> ChangedPixels { get; set; } 


        public BitmapPixelChanges(Dictionary<Coordinates, Color> changedPixels)
        {
            ChangedPixels = changedPixels;
        }

        public static BitmapPixelChanges FromSingleColoredArray(Coordinates[] coordinates, Color color)
        {
            Dictionary<Coordinates, Color> dict = new Dictionary<Coordinates, Color>();
            for (int i = 0; i < coordinates.Length; i++)
            {
                dict.Add(coordinates[i], color);
            }
            return new BitmapPixelChanges(dict);
        }

        public static BitmapPixelChanges FromArrays(IEnumerable<Coordinates> coordinates, IEnumerable<Color> color)
        {
            var coordinateArray = coordinates.ToArray();
            var colorArray = color.ToArray();
            if(coordinateArray.Length != colorArray.Length)
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
