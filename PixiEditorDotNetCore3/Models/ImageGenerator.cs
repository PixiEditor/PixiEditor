using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace PixiEditorDotNetCore3.Models
{
    public static class ImageGenerator
    {
        public static Image GenerateForPixelArts(int width, int height)
        {
            Image image = new Image();
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(image, EdgeMode.Aliased);
            image.Stretch = Stretch.Uniform;
            image.Width = width;
            image.Height = height;
            return image;
        }
    }
}
