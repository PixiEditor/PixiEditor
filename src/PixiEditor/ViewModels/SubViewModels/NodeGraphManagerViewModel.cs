using Avalonia.Input;
using PixiEditor.Models.AnalyticsAPI;
using PixiEditor.ViewModels.Nodes;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Handlers;
using Drawie.Numerics;
using PixiEditor.ViewModels.Dock;

namespace PixiEditor.ViewModels.SubViewModels;

internal class NodeGraphManagerViewModel : SubViewModel<ViewModelMain>
{
    public NodeGraphManagerViewModel(ViewModelMain owner) : base(owner)
    {
    }

    [Command.Basic("PixiEditor.NodeGraph.DeleteSelectedNodes", "DELETE_NODES", "DELETE_NODES_DESCRIPTIVE", 
        Key = Key.Delete, ShortcutContexts = [typeof(NodeGraphDockViewModel)], AnalyticsTrack = true)]
    public void DeleteSelectedNodes()
    {
        var nodes = Owner.DocumentManagerSubViewModel.ActiveDocument?.NodeGraph.AllNodes
            .Where(x => x.IsNodeSelected).ToList();
        
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
    public void ConnectProperties((INodePropertyHandler input, INodePropertyHandler output, INodePropertyHandler? inputToDisconnect) args)
    {
        using var changeBlock = Owner.DocumentManagerSubViewModel.ActiveDocument.Operations.StartChangeBlock();
        if (args.inputToDisconnect != null)
        {
            Owner.DocumentManagerSubViewModel.ActiveDocument.NodeGraph.ConnectProperties(args.inputToDisconnect, null);
        }
        
        Owner.DocumentManagerSubViewModel.ActiveDocument?.NodeGraph.ConnectProperties(args.input, args.output);
    }

    [Command.Internal("PixiEditor.NodeGraph.ChangeNodePos")]
    public void ChangeNodePos((List<INodeHandler> nodes, VecD newPos) args)
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.NodeGraph.SetNodePositions(args.nodes, args.newPos);
    }

    [Command.Internal("PixiEditor.NodeGraph.UpdateValue", AnalyticsTrack = true)]
    public void UpdatePropertyValue((INodeHandler node, string property, object value) args)
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.NodeGraph.UpdatePropertyValue(args.node, args.property,
            args.value);
    }

    [Command.Internal("PixiEditor.NodeGraph.BeginUpdateValue", AnalyticsTrack = true)]
    public void BeginUpdatePropertyValue((INodeHandler node, string property, object value) args)
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.NodeGraph.BeginUpdatePropertyValue(args.node, args.property,
            args.value);
    }

    [Command.Internal("PixiEditor.NodeGraph.EndUpdateValue", AnalyticsTrack = true)]
    public void EndUpdatePropertyValue()
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.NodeGraph.EndUpdatePropertyValue();
    }

    [Command.Internal("PixiEditor.NodeGraph.EndChangeNodePos")]
    public void EndChangeNodePos()
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.NodeGraph.EndChangeNodePosition();
    }

    [Command.Internal("PixiEditor.NodeGraph.GetComputedPropertyValue")]
    public void GetComputedPropertyValue(INodePropertyHandler property)
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.NodeGraph.RequestUpdateComputedPropertyValue(property);
    }

    [Command.Internal("PixiEditor.NodeGraph.AddVariable")]
    public void AddVariable(Type type)
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.NodeGraph.Blackboard.AddVariable(type);
    }
}
