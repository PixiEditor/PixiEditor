using PixiEditor.Models.Position;
using PixiEditor.ViewModels;
using SkiaSharp;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PixiEditor.Models.Tools.Tools
{
    public class ColorPickerTool : ReadonlyTool
    {
        public ColorPickerTool()
        {
            ActionDisplay = "Press on pixel to make it the primary color.";
        }

        public override bool HideHighlight => true;

        public override string Tooltip => "Swaps primary color with selected on canvas. (O)";


        public override void Use(List<Coordinates> coordinates)
        {
            var coords = coordinates.First();
            ViewModelMain.Current.ColorsSubViewModel.PrimaryColor = GetColorAt(coords.X, coords.Y);
        }

        public SKColor GetColorAt(int x, int y)
        {
            var color = ViewModelMain.Current.BitmapManager?.ActiveDocument?.Renderer?.FinalSurface.GetSRGBPixel(x, y);
            return color.HasValue ? color.Value : SKColors.Transparent;
        }

        public SKColor GetColorUnderMouse()
        {
            System.Drawing.Color color;
            using (Bitmap bitmap = new Bitmap(1, 1))
            {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(MousePositionConverter.GetCursorPosition(), new Point(0, 0), new Size(1, 1));
                }

                color = bitmap.GetPixel(0, 0);
            }

            return new SKColor(color.R, color.G, color.B, color.A);
        }
    }
}
