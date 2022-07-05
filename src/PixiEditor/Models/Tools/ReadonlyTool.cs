using ChunkyImageLib.DataHolders;

namespace PixiEditor.Models.Tools;

internal abstract class ReadonlyTool : Tool
{
    public abstract void Use(VecD position);
}
