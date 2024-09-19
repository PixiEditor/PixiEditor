using PixiEditor.Numerics;

namespace PixiEditor.Models.Handlers;

public interface ISnappingHandler
{
    public void Remove(string id);
    public void AddFromBounds(string id, Func<RectD> tightBounds);
}
