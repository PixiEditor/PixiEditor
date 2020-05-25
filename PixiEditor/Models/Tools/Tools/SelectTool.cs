using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixiEditor.Models.Tools.Tools
{
    public class SelectTool : ReadonlyTool
    {
        public override ToolType ToolType => ToolType.Select;

        public override void Use(Coordinates[] pixels)
        {
            RectangleTool rectangleTool = new RectangleTool();
            List<Coordinates> selection = rectangleTool.CreateRectangle(pixels, 1).ToList();
            selection.AddRange(rectangleTool.CalculateFillForRectangle(selection[0], selection[^1], 1));
            ViewModelMain.Current.ActiveSelection = new DataHolders.Selection(selection.ToArray());
        }
    }
}
