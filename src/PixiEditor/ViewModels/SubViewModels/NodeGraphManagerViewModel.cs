using Avalonia.Input;
using PixiEditor.ViewModels.Nodes;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Handlers;
using PixiEditor.Numerics;
using PixiEditor.ViewModels.Dock;

namespace PixiEditor.ViewModels.SubViewModels;

internal class NodeGraphManagerViewModel : SubViewModel<ViewModelMain>
{
    public NodeGraphManagerViewModel(ViewModelMain owner) : base(owner)
    {
    }

    [Command.Basic("PixiEditor.NodeGraph.DeleteSelectedNodes", "DELETE_NODES", "DELETE_NODES_DESCRIPTIVE", 
        Key = Key.Delete, ShortcutContext = typeof(NodeGraphDockViewModel))]
    public void DeleteSelectedNodes()
    {
        Guid[] selectedNodes = Owner.DocumentManagerSubViewModel.ActiveDocument?.NodeGraph.AllNodes
            .Where(x => x.IsSelected).Select(x => x.Id).ToArray();
        
        if (selectedNodes == null || selectedNodes.Length == 0)
            return;

        Owner.DocumentManagerSubViewModel.ActiveDocument?.NodeGraph.RemoveNodes(selectedNodes);
    }

    [Command.Debug("PixiEditor.NodeGraph.CreateNodeFrameAroundEverything", "Create node frame", "Create node frame")]
    public void CreateNodeFrameAroundEverything()
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.NodeGraph.CreateNodeFrameAroundEverything();
    }

    [Command.Internal("PixiEditor.NodeGraph.CreateNode")]
    public void CreateNode((Type nodeType, VecD pos) data)
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.NodeGraph.CreateNode(data.nodeType, data.pos);
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

    [Command.Internal("PixiEditor.NodeGraph.UpdateValue")]
    public void UpdatePropertyValue((INodeHandler node, string property, object value) args)
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.NodeGraph.UpdatePropertyValue(args.node, args.property,
            args.value);
    }

    [Command.Internal("PixiEditor.NodeGraph.EndChangeNodePos")]
    public void EndChangeNodePos()
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.NodeGraph.EndChangeNodePosition();
    }
}
