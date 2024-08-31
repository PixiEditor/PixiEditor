using Avalonia.Input;
using PixiEditor.Models.AnalyticsAPI;
using PixiEditor.ViewModels.Nodes;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Handlers;
using PixiEditor.Numerics;
using PixiEditor.ViewModels.Dock;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.ViewModels.SubViewModels;

internal class NodeGraphManagerViewModel : SubViewModel<ViewModelMain>
{
    public NodeGraphManagerViewModel(ViewModelMain owner) : base(owner)
    {
    }

    [Command.Basic("PixiEditor.NodeGraph.DeleteSelectedNodes", "DELETE_NODES", "DELETE_NODES_DESCRIPTIVE", 
        Key = Key.Delete, ShortcutContext = typeof(NodeGraphDockViewModel), AnalyticsTrack = true)]
    public void DeleteSelectedNodes()
    {
        var nodes = Owner.DocumentManagerSubViewModel.ActiveDocument?.NodeGraph.AllNodes
            .Where(x => x.IsSelected).ToList();
        
        if (nodes == null || nodes.Count == 0)
            return;
        
        for (var i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            if(node.Metadata?.PairNodeGuid == null) continue;
            
            INodeHandler otherNode = Owner.DocumentManagerSubViewModel.ActiveDocument?.NodeGraph.AllNodes
                .FirstOrDefault(x => x.Id == node.Metadata.PairNodeGuid);
            
            if (otherNode != null && !nodes.Contains(otherNode))
            {
                nodes.Add(otherNode);
            }
        }
        
        Guid[] selectedNodes = nodes.Select(x => x.Id).ToArray();

        if (selectedNodes == null || selectedNodes.Length == 0)
            return;

        Owner.DocumentManagerSubViewModel.ActiveDocument?.NodeGraph.RemoveNodes(selectedNodes);
    }

    [Command.Basic("PixiEditor.NodeGraph.CreateConstant", "CREATE_NODE_CONSTANT", "CREATE_NODE_CONSTANT", ShortcutContext = typeof(NodeGraphDockViewModel), AnalyticsTrack = true)]
    public void CreateConstant()
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.NodeGraph.CreateConstant(Guid.NewGuid(), typeof(double));
    }

    // TODO: Remove this
    [Command.Debug("PixiEditor.NodeGraph.CreateNodeFrameAroundEverything", "Create node frame", "Create node frame", AnalyticsTrack = true)]
    public void CreateNodeFrameAroundEverything()
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.NodeGraph.CreateNodeFrameAroundEverything();
    }

    [Command.Internal("PixiEditor.NodeGraph.CreateNode")]
    public void CreateNode((Type nodeType, VecD pos) data)
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.NodeGraph.CreateNode(data.nodeType, data.pos);
        Analytics.SendCreateNode(data.nodeType);
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

    [Command.Internal("PixiEditor.NodeGraph.UpdateValue", AnalyticsTrack = true)]
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

    [Command.Internal("PixiEditor.NodeGraph.CreateConstantNode", AnalyticsTrack = true)]
    public void CreateConstantNode(Guid id)
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.NodeGraph.CreateConstantNode(Guid.NewGuid(), id);
    }

    public void UpdateConstantValue((INodeGraphConstantHandler constant, object value) args)
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.NodeGraph.UpdateConstantValue(args.constant, args.value);
    }
}
