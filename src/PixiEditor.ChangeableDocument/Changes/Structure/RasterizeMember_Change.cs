using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.ChangeableDocument.ChangeInfos.Animation;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;
using Drawie.Backend.Core;
using Drawie.Numerics;
using Drawie.Backend.Core.Surfaces;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changes.Animation;

namespace PixiEditor.ChangeableDocument.Changes.Structure;

internal class RasterizeMember_Change : Change
{
    private Guid memberId;
    private Guid createdNodeId;
    
    private Node originalNode;

    private int frame = 0;

    private ConnectionsData originalConnections;
    
    [GenerateMakeChangeAction]
    public RasterizeMember_Change(Guid memberId, Guid newMemberId, int frame)
    {
        this.memberId = memberId;
        this.createdNodeId = newMemberId;
        this.frame = frame;
    }
    
    public override bool InitializeAndValidate(Document target)
    {
        if (target.TryFindMember(memberId, out var member) 
            && member is not IReadOnlyImageNode && member is IRasterizable)
        {
            originalNode = member.Clone(true);
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
        imageLayer.Id = createdNodeId;

        target.NodeGraph.AddNode(imageLayer);

        if (node is VectorLayerNode vector && vector.KeyFrames.Count > 1)
        {
            imageLayer.FallbackAnimationToLayerImage = vector.FallbackAnimationToLayerImage;
            foreach (var keyFrame in vector.KeyFrames)
            {
                ChunkyImage celImage;
                if (keyFrame == vector.KeyFrames[0])
                {
                    celImage = imageLayer.GetLayerImageAtFrame(0);
                }
                else
                {
                    celImage = (ChunkyImage)imageLayer.CreateEmptyCelData(target.Size);
                    imageLayer.AddFrame(keyFrame.KeyFrameGuid,
                        new KeyFrameData(
                            keyFrame.KeyFrameGuid, keyFrame.StartFrame, keyFrame.Duration, ImageLayerNode.ImageLayerKey)
                        {
                            Data = celImage,
                            IsVisible = keyFrame.IsVisible,
                        });
                }
                DrawInto(target, celImage, canvas => vector.RasterizeKeyFrame(canvas, null, keyFrame));
            }
        } else
        {
            DrawInto(target, imageLayer.GetLayerImageAtFrame(0), canvas => rasterizable.Rasterize(canvas, null, frame));
        }

        OutputProperty<Painter>? outputConnection = node.OutputProperties.FirstOrDefault(x => x is OutputProperty<Painter>) as OutputProperty<Painter>;
        InputProperty<Painter>? outputConnectedInput =
            outputConnection?.Connections.FirstOrDefault(x => x is InputProperty<Painter>) as InputProperty<Painter>;

        InputProperty<Painter> backgroundInput = imageLayer.Background;
        OutputProperty<Painter> toAddOutput = imageLayer.Output;

        List<IChangeInfo> changeInfos = new();
        if (target.AnimationData.TryFindKeyFrame<GroupKeyFrame>(memberId, out _))
        {
            target.AnimationData.RemoveKeyFrame(memberId);
            changeInfos.Add(new DeleteKeyFrame_ChangeInfo(memberId));
        }
        changeInfos.Add(CreateNode_ChangeInfo.CreateFromNode(imageLayer));
        changeInfos.AddRange(CreateAnimationDataFromLayer_Change.CreateTimelineRow(target, imageLayer));
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
        
        changeInfos.AddRange(NodeOperations.DetachNode(node));
        
        node.Dispose();
        target.NodeGraph.RemoveNode(node);
        
        changeInfos.Add(new DeleteNode_ChangeInfo(node.Id));

        ignoreInUndo = false;
        return changeInfos;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        Node node = target.FindMember(createdNodeId);
        
        List<IChangeInfo> changeInfos = new();

        if (target.AnimationData.TryFindKeyFrame<GroupKeyFrame>(createdNodeId, out _))
        {
            target.AnimationData.RemoveKeyFrame(createdNodeId);
            changeInfos.Add(new DeleteKeyFrame_ChangeInfo(createdNodeId));
        }

        changeInfos.AddRange(NodeOperations.DetachNode(node));
        
        node.Dispose();
        target.NodeGraph.RemoveNode(node);
        
        changeInfos.Add(new DeleteNode_ChangeInfo(node.Id));
        
        var restoredNode = originalNode.Clone(true);
        restoredNode.Id = memberId;

        target.NodeGraph.AddNode(restoredNode);

        changeInfos.Add(CreateNode_ChangeInfo.CreateFromNode(restoredNode));
        if (restoredNode is LayerNode restoredLayer)
        {
            changeInfos.AddRange(CreateAnimationDataFromLayer_Change.CreateTimelineRow(target, restoredLayer));
        }
        
        changeInfos.AddRange(NodeOperations.ConnectStructureNodeProperties(originalConnections, restoredNode, target.NodeGraph));
        
        return changeInfos;   
    }

    public override void Dispose()
    {
        originalNode?.Dispose();
    }

    private static void DrawInto(Document target, ChunkyImage image, Action<Canvas> rasterize)
    {
        using Surface surface = Surface.ForProcessing(target.Size, target.ProcessingColorSpace);
        rasterize(surface.DrawingSurface.Canvas);

        image.EnqueueDrawImage(VecI.Zero, surface);
        image.CommitChanges();
    }
}
