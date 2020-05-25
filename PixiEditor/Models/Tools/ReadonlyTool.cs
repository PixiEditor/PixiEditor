using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using System;
using System.Collections.Generic;
using System.Text;

namespace PixiEditor.Models.Tools
{
    public abstract class ReadonlyTool : Tool
    {
        public abstract void Use(Coordinates[] pixels);
    }
}
