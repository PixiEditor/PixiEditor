using System.Windows.Controls;
using System.Windows.Media;

namespace PixiEditor.Models.Images
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