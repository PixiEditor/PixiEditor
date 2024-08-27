using PixiEditor.Common;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Points;

public class PointList : List<VecD>, ICacheable, ICloneable
{
    public required int HashValue { get; set; }

    public PointList()
    {
    }

    public PointList(IEnumerable<VecD> collection) : base(collection)
    {
    }

    public PointList(int capacity) : base(capacity)
    {
    }

    public static PointList Empty { get; } = new(0) { HashValue = 0 };

    public int GetCacheHash() => HashValue;
    public object Clone()
    {
        var clone = new PointList(this) { HashValue = HashValue };
        clone.HashValue = HashValue;
        return clone;
    }
}
