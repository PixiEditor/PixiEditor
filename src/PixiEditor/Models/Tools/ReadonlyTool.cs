using ChunkyImageLib.DataHolders;

namespace PixiEditor.Models.Tools;

public abstract class ReadonlyTool : Tool
{
    public abstract void Use(VecD position);
}
