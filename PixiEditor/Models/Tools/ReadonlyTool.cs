using PixiEditor.Models.Position;

namespace PixiEditor.Models.Tools
{
    public abstract class ReadonlyTool : Tool
    {
        public abstract void Use(Coordinates[] pixels);
    }
}
