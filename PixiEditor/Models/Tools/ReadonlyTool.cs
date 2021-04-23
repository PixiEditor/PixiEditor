using System.Collections.Generic;
using PixiEditor.Models.Position;

namespace PixiEditor.Models.Tools
{
    public abstract class ReadonlyTool : Tool
    {
        public abstract void Use(List<Coordinates> pixels);
    }
}