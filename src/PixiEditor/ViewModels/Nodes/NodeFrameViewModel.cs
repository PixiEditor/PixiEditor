using PixiEditor.Models.Handlers;

namespace PixiEditor.ViewModels.Nodes;

internal sealed class NodeFrameViewModel : NodeFrameViewModelBase
{
    public NodeFrameViewModel(Guid id, IEnumerable<INodeHandler> nodes) : base(id, nodes)
    {
        CalculateBounds();
    }

    protected override void CalculateBounds()
    {
        throw new NotImplementedException();
    }
}
