using PixiEditor.Models.Position;
using System.Collections.Generic;

namespace PixiEditor.Models.Tools;

public abstract class ReadonlyTool : Tool
{
    public abstract void Use(IReadOnlyList<Coordinates> pixels);
}