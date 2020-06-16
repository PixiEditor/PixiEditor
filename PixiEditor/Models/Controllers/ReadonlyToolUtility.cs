using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;

namespace PixiEditor.Models.Controllers
{
    public class ReadonlyToolUtility
    {
        public ReadonlyToolUtility(BitmapManager manager)
        {
            Manager = manager;
        }

        public BitmapManager Manager { get; set; }

        public void ExecuteTool(Coordinates[] mouseMove, ReadonlyTool tool)
        {
            tool.Use(mouseMove);
        }
    }
}