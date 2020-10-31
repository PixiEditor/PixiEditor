using PixiEditor.Models.Position;

namespace PixiEditor.Models.Tools
{
    public abstract class ReadonlyTool : Tool
    {
        public virtual void Use(Coordinates[] pixels) { }
    }
}