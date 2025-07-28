using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.Models.Handlers;
using Drawie.Numerics;

namespace PixiEditor.ViewModels.Nodes;

internal sealed class NodeFrameViewModel : NodeFrameViewModelBase
{
    public NodeFrameViewModel(Guid id, IEnumerable<INodeHandler> nodes) : base(id, nodes)
    {
        CalculateBounds();
    }

    protected override void CalculateBounds()
    {
        
        // TODO: Use the GetBounds like in NodeZoneViewModel
        if (Nodes.Count == 0)
        {
            if (TopLeft == BottomRight)
            {
                BottomRight = TopLeft + new VecD(100, 100);
            }
            
            return;
        }
        
        var minX = Nodes.Min(n => n.PositionBindable.X) - 30;
        var minY = Nodes.Min(n => n.PositionBindable.Y) - 45;
        
        var maxX = Nodes.Max(n => n.PositionBindable.X) + 130;
        var maxY = Nodes.Max(n => n.PositionBindable.Y) + 130;

        TopLeft = new VecD(minX, minY);
        BottomRight = new VecD(maxX, maxY);

        Size = BottomRight - TopLeft;
    }
}
