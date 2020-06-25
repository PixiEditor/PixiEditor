using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;

namespace PixiEditor.Models.Controllers
{
    public class ReadonlyToolUtility
    {
        public void ExecuteTool(Coordinates[] mouseMove, ReadonlyTool tool)
        {
            tool.Use(mouseMove);
        }
    }
}