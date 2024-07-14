using PixiEditor.AvaloniaUI.Models.Commands.Attributes.Commands;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.Numerics;

namespace PixiEditor.AvaloniaUI.ViewModels.SubViewModels;

internal class NodeGraphManagerViewModel : SubViewModel<ViewModelMain>
{
    public NodeGraphManagerViewModel(ViewModelMain owner) : base(owner)
    {
    }

    [Command.Debug("PixiEditor.NodeGraph.CreateNodeFrameAroundEverything", "Create node frame", "Create node frame")]
    public void CreateNodeFrameAroundEverything()
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.NodeGraph.CreateNodeFrameAroundEverything();
    }

    [Command.Internal("PixiEditor.NodeGraph.CreateNode")]
    public void CreateNode(Type nodeType)
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.NodeGraph.CreateNode(nodeType);
    }

    [Command.Internal("PixiEditor.NodeGraph.ConnectProperties")]
    public void ConnectProperties((INodePropertyHandler input, INodePropertyHandler output) args)
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.NodeGraph.ConnectProperties(args.input, args.output);
    }

    [Command.Internal("PixiEditor.NodeGraph.ChangeNodePos")]
    public void ChangeNodePos((INodeHandler node, VecD newPos) args)
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.NodeGraph.SetNodePosition(args.node, args.newPos);
    }
    
    [Command.Internal("PixiEditor.NodeGraph.EndChangeNodePos")]
    public void EndChangeNodePos()
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.NodeGraph.EndChangeNodePosition();
    }
}
