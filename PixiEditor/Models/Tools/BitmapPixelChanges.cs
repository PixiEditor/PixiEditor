using PixiEditor.Models.Position;
using System;
using System.Collections.Generic;
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
    }
}
