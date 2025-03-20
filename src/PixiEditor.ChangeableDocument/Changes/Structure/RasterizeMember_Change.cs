using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Structure;

internal class RasterizeMember_Change : Change
{
    private Guid memberId;
    
    private Node originalNode;
    private Guid createdNodeId;
    
    private ConnectionsData originalConnections;
    
    [GenerateMakeChangeAction]
    public RasterizeMember_Change(Guid memberId)
    {
        this.memberId = memberId;
    }
    
    public override bool InitializeAndValidate(Document target)
    {
        if (target.TryFindMember(memberId, out var member) 
            && member is not IReadOnlyImageNode && member is IRasterizable)
        {
            originalNode = member.Clone();
            originalConnections = NodeOperations.CreateConnectionsData(member);
            return true;
        }
        
        return false;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        Node node = target.FindMember(memberId);
        
        IRasterizable rasterizable = (IRasterizable)node;
        
        ImageLayerNode imageLayer = new ImageLayerNode(target.Size, target.ProcessingColorSpace);
        imageLayer.MemberName = node.DisplayName;
        imageLayer.Position = node.Position;

        target.NodeGraph.AddNode(imageLayer);
        
        using Surface surface = Surface.ForProcessing(target.Size, target.ProcessingColorSpace);
        rasterizable.Rasterize(surface.DrawingSurface, null);
        
        var image = imageLayer.GetLayerImageAtFrame(0);
        image.EnqueueDrawImage(VecI.Zero, surface);
        image.CommitChanges();

        OutputProperty<Painter>? outputConnection = node.OutputProperties.FirstOrDefault(x => x is OutputProperty<Painter>) as OutputProperty<Painter>;
        InputProperty<Painter>? outputConnectedInput =
            outputConnection?.Connections.FirstOrDefault(x => x is InputProperty<Painter>) as InputProperty<Painter>;

        InputProperty<Painter> backgroundInput = imageLayer.Background;
        OutputProperty<Painter> toAddOutput = imageLayer.Output;

        List<IChangeInfo> changeInfos = new();
        changeInfos.Add(CreateNode_ChangeInfo.CreateFromNode(imageLayer));
        changeInfos.AddRange(NodeOperations.AppendMember(outputConnectedInput, toAddOutput, backgroundInput, imageLayer.Id));

        List<(string inputPropName, IOutputProperty connection)> connections = new();
        
        foreach (var inputProp in node.InputProperties)
        {
            if(inputProp.Connection == null) continue;
            
            connections.Add((inputProp.InternalPropertyName, inputProp.Connection));
        }

        foreach (var conn in connections)
        {
            InputProperty? targetInput = imageLayer.GetInputProperty(conn.inputPropName);
            if (targetInput == null) continue;
            
            conn.connection.ConnectTo(targetInput);
            changeInfos.Add(new ConnectProperty_ChangeInfo(conn.connection.Node.Id, imageLayer.Id, conn.connection.InternalPropertyName, conn.inputPropName));
        }
        
        changeInfos.AddRange(NodeOperations.DetachNode(target.NodeGraph, node));
        
        node.Dispose();
        target.NodeGraph.RemoveNode(node);
        
        changeInfos.Add(new DeleteNode_ChangeInfo(node.Id));
        
        createdNodeId = imageLayer.Id;
        
        ignoreInUndo = false;
        return changeInfos;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        Node node = target.FindMember(createdNodeId);
        
        List<IChangeInfo> changeInfos = new();
        changeInfos.AddRange(NodeOperations.DetachNode(target.NodeGraph, node));
        
        node.Dispose();
        target.NodeGraph.RemoveNode(node);
        
        changeInfos.Add(new DeleteNode_ChangeInfo(node.Id));
        
        var restoredNode = originalNode.Clone();
        restoredNode.Id = memberId;
        
        target.NodeGraph.AddNode(restoredNode);
        
        changeInfos.Add(CreateNode_ChangeInfo.CreateFromNode(restoredNode));
        
        changeInfos.AddRange(NodeOperations.ConnectStructureNodeProperties(originalConnections, restoredNode, target.NodeGraph));
        
        return changeInfos;   
    }

    public override void Dispose()
    {
        originalNode.Dispose();
    }
}
