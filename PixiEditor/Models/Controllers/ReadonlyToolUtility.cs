using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
using System.Collections.Generic;

namespace PixiEditor.Models.Controllers
{
    public class ReadonlyToolUtility
    {
        public BitmapManager Manager { get; set; }

        public ReadonlyToolUtility(BitmapManager manager)
        {
            Manager = manager;
        }

        public void ExecuteTool(Coordinates[] mouseMove, ReadonlyTool tool)
        {            
            tool.Use(mouseMove);
        }

    }
}
