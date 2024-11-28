using PixiEditor.Models.Handlers;
using Drawie.Numerics;

namespace PixiEditor.ViewModels.Nodes;

public sealed class NodeZoneViewModel : NodeFrameViewModelBase
{
    private INodeHandler start;
    private INodeHandler end;
    
    public NodeZoneViewModel(Guid id, string internalName, INodeHandler start, INodeHandler end) : base(id, [start, end])
    {
        InternalName = internalName;
        
        this.start = start.Metadata.IsPairNodeStart ? start : end;
        this.end = start.Metadata.IsPairNodeStart ? end : start;
        
        CalculateBounds();
    }

    protected override void CalculateBounds()
    {
        if (Nodes.Count == 0)
        {
            if (TopLeft == BottomRight)
            {
                BottomRight = TopLeft + new VecD(100, 100);
            }
            
            return;
        }

        var bounds = GetBounds();
        
        var minX = bounds.Min(n => n.X);
        var minY = bounds.Min(n => n.Y);
        
        var maxX = bounds.Max(n => n.Right);
        var maxY = bounds.Max(n => n.Bottom);

        TopLeft = new VecD(minX, minY);
        BottomRight = new VecD(maxX, maxY);

        Size = BottomRight - TopLeft;
    }

    private List<RectD> GetBounds()
    {
        var list = new List<RectD>();

        const int defaultXOffset = -30;
        const int defaultYOffset = -45;
        
        // TODO: Use the actual node height
        foreach (var node in Nodes)
        {
            if (node == start)
            {
                list.Add(new RectD(node.PositionBindable + new VecD(100, defaultYOffset), new VecD(100, 400)));
                continue;
            }

            if (node == end)
            {
                list.Add(new RectD(node.PositionBindable + new VecD(defaultXOffset, defaultYOffset), new VecD(100, 400)));
                continue;
            }
            
            list.Add(new RectD(node.PositionBindable + new VecD(defaultXOffset, defaultYOffset), new VecD(200, 400)));
        }
        
        return list;
    }
}
