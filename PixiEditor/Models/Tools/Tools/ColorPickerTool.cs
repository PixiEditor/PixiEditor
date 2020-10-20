using System.Drawing;
using PixiEditor.Models.Position;
using PixiEditor.ViewModels;
using Color = System.Windows.Media.Color;

namespace PixiEditor.Models.Tools.Tools
{
    public class ColorPickerTool : ReadonlyTool
    {
        public ColorPickerTool()
        {
            HideHighlight = true;
            Tooltip = "Swaps primary color with selected on canvas. (O)";
        }

        public override ToolType ToolType => ToolType.ColorPicker;

        public override void Use(Coordinates[] coordinates)
        {
            ViewModelMain.Current.PrimaryColor = GetColorUnderMouse();
        }

        public Color GetColorUnderMouse()
        {
            System.Drawing.Color color;
            using (var bitmap = new Bitmap(1, 1))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(MousePositionConverter.GetCursorPosition(), new Point(0, 0),
                        new Size(1, 1));
                }

                color = bitmap.GetPixel(0, 0);
            }

            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}