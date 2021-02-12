using System.Drawing;
using System.Windows.Input;
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
            ActionDisplay = "Press on pixel to make it the primary color.";
            Tooltip = "Swaps primary color with selected on canvas. (O)";
        }

        public override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            ViewModelMain.Current.ColorsSubViewModel.PrimaryColor = GetColorUnderMouse();
        }

        public override void Use(Coordinates[] coordinates) { }

        public Color GetColorUnderMouse()
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

            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}