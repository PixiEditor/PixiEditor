using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Points;

public class PointList : List<VecD>
{
    public PointList()
    {
    }

    public PointList(IEnumerable<VecD> collection) : base(collection)
    {
    }

    public PointList(int capacity) : base(capacity)
    {
    }

    public static PointList Empty { get; } = new(0);
}
