using System.Collections.Generic;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;

namespace PixiEditor.Models.Controllers
{
    public class ReadonlyToolUtility
    {
        public void ExecuteTool(List<Coordinates> mouseMove, ReadonlyTool tool)
        {
            tool.Use(mouseMove);
        }
    }
}