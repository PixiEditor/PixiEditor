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
        
        foreach (var node in Nodes)
        {
            var size = new VecD(node.UiSize.Size.Width, node.UiSize.Size.Height);

            if (node == start)
            {
                // The + 40 ensure that the rectangle has a minimum size
                var safeSizeOffset = size + new VecD(-size.X + 40, defaultYOffset * -2);
                
                list.Add(new RectD(node.PositionBindable + new VecD(size.X / 3 * 2, defaultYOffset), safeSizeOffset));
                continue;
            }
            
            if (node == end)
            {
                var sizeOffset = size + new VecD(-size.X, defaultYOffset * -2);

                list.Add(new RectD(node.PositionBindable + new VecD(size.X / 3, defaultYOffset), sizeOffset));
                continue;
            }
            
            var sizeNormalOffset = size + new VecD(defaultXOffset * -2, defaultYOffset * -2);

            list.Add(new RectD(node.PositionBindable + new VecD(defaultXOffset, defaultYOffset), sizeNormalOffset));
        }
        
        return list;
    }
}
