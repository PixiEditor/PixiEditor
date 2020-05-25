using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace PixiEditor.Models.Tools.Tools
{
    public class ColorPickerTool : ReadonlyTool
    {
        public override ToolType ToolType => ToolType.ColorPicker;

        public ColorPickerTool()
        {
            HideHighlight = true;
        }

        public override void Use()
        {
            using (var bitmap = new System.Drawing.Bitmap(1, 1))
            {
                using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(MousePositionConverter.GetCursorPosition(), new System.Drawing.Point(0, 0), new System.Drawing.Size(1, 1));
                }
                var color = bitmap.GetPixel(0, 0);
                ViewModelMain.Current.PrimaryColor = Color.FromArgb(color.A, color.R, color.G, color.B);
            }
        }
    }
}
