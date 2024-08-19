using System.Collections.Immutable;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Changeables.Graph.Exceptions;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.ChangeInfos.Animation;
using PixiEditor.ChangeableDocument.ChangeInfos.Drawing;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.ChangeableDocument.ChangeInfos.Properties;
using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using PixiEditor.ChangeableDocument.ChangeInfos.Root.ReferenceLayerChangeInfos;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core;
using PixiEditor.Helpers;
using PixiEditor.Models.DocumentPassthroughActions;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Layers;
using PixiEditor.Numerics;
using PixiEditor.ViewModels.Document;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.Models.DocumentModels;
#nullable enable
internal class DocumentUpdater
{
    private IDocument doc;
    private DocumentInternalParts helper;

    public DocumentUpdater(IDocument doc, DocumentInternalParts helper)
    {
        this.doc = doc;
        this.helper = helper;
    }

    /// <summary>
    /// Don't call this outside ActionAccumulator
    /// </summary>
    public void AfterUndoBoundaryPassed()
    {
        //TODO: Make sure AllChangesSaved trigger raise property changed itself
        doc.UpdateSavedState();
    }

    /// <summary>
    /// Don't call this outside ActionAccumulator
    /// </summary>
    public void ApplyChangeFromChangeInfo(IChangeInfo arbitraryInfo)
    {
        if (arbitraryInfo is null)
            return;

        //TODO: Find a more elegant way to do this
        switch (arbitraryInfo)
        {
            case CreateStructureMember_ChangeInfo info:
                if (info is CreateLayer_ChangeInfo layerChangeInfo)
                {
                    ProcessCreateNode<LayerViewModel>(info);
                }
                else if (info is CreateFolder_ChangeInfo folderChangeInfo)
                {
                    ProcessCreateNode<FolderViewModel>(info);
                }

                ProcessCreateStructureMember(info);
                break;
            case DeleteStructureMember_ChangeInfo info:
                ProcessDeleteStructureMember(info);
                ProcessDeleteNode(info);
                break;
            case StructureMemberName_ChangeInfo info:
                ProcessUpdateStructureMemberName(info);
                break;
            case StructureMemberIsVisible_ChangeInfo info:
                ProcessUpdateStructureMemberIsVisible(info);
                break;
            case StructureMemberOpacity_ChangeInfo info:
                ProcessUpdateStructureMemberOpacity(info);
                break;
            case MoveStructureMember_ChangeInfo info:
                ProcessMoveStructureMember(info);
                break;
            case Size_ChangeInfo info:
                ProcessSize(info);
                break;
            case RefreshViewport_PassthroughAction info:
                ProcessRefreshViewport(info);
                break;
            case RemoveViewport_PassthroughAction info:
                ProcessRemoveViewport(info);
                break;
            case StructureMemberMask_ChangeInfo info:
                ProcessStructureMemberMask(info);
                break;
            case StructureMemberBlendMode_ChangeInfo info:
                ProcessStructureMemberBlendMode(info);
                break;
            case LayerLockTransparency_ChangeInfo info:
                ProcessLayerLockTransparency(info);
                break;
            case Selection_ChangeInfo info:
                ProcessSelection(info);
                break;
            case SymmetryAxisState_ChangeInfo info:
                ProcessSymmetryState(info);
                break;
            case SymmetryAxisPosition_ChangeInfo info:
                ProcessSymmetryPosition(info);
                break;
            case StructureMemberClipToMemberBelow_ChangeInfo info:
                ProcessClipToMemberBelow(info);
                break;
            case StructureMemberMaskIsVisible_ChangeInfo info:
                ProcessMaskIsVisible(info);
                break;
            case SetReferenceLayer_ChangeInfo info:
                ProcessSetReferenceLayer(info);
                break;
            case DeleteReferenceLayer_ChangeInfo info:
                ProcessDeleteReferenceLayer(info);
                break;
            case TransformReferenceLayer_ChangeInfo info:
                ProcessTransformReferenceLayer(info);
                break;
            case ReferenceLayerIsVisible_ChangeInfo info:
                ProcessReferenceLayerIsVisible(info);
                break;
            case ReferenceLayerTopMost_ChangeInfo info:
                ProcessReferenceLayerTopMost(info);
                break;
            case SetSelectedMember_PassthroughAction info:
                ProcessSetSelectedMember(info);
                break;
            case AddSoftSelectedMember_PassthroughAction info:
                ProcessAddSoftSelectedMember(info);
                break;
            case RemoveSoftSelectedMember_PassthroughAction info:
                ProcessRemoveSoftSelectedMember(info);
                break;
            case ClearSoftSelectedMembers_PassthroughAction info:
                ProcessClearSoftSelectedMembers(info);
                break;
            case CreateRasterKeyFrame_ChangeInfo info:
                ProcessCreateRasterKeyFrame(info);
                break;
            case DeleteKeyFrame_ChangeInfo info:
                ProcessDeleteKeyFrame(info);
                break;
            case SetActiveFrame_PassthroughAction info:
                ProcessActiveFrame(info);
                break;
            case ToggleOnionSkinning_PassthroughAction info:
                ProcessToggleOnionSkinning(info);
                break;
            case KeyFrameLength_ChangeInfo info:
                ProcessKeyFrameLength(info);
                break;
            case KeyFrameVisibility_ChangeInfo info:
                ProcessKeyFrameVisibility(info);
                break;
            case AddSelectedKeyFrame_PassthroughAction info:
                ProcessAddSelectedKeyFrame(info);
                break;
            case RemoveSelectedKeyFrame_PassthroughAction info:
                ProcessRemoveSelectedKeyFrame(info);
                break;
            case ClearSelectedKeyFrames_PassthroughAction info:
                ClearSelectedKeyFrames(info);
                break;
            case CreateNode_ChangeInfo info:
                ProcessCreateNode<NodeViewModel>(info);
                break;
            case DeleteNode_ChangeInfo info:
                ProcessDeleteNode(info);
                break;
            case CreateNodeFrame_ChangeInfo info:
                ProcessCreateNodeFrame(info);
                break;
            case CreateNodeZone_ChangeInfo info:
                ProcessCreateNodeZone(info);
                break;
            case DeleteNodeFrame_ChangeInfo info:
                ProcessDeleteNodeFrame(info);
                break;
            case ConnectProperty_ChangeInfo info:
                ProcessConnectProperty(info);
                break;
            case NodePosition_ChangeInfo info:
                ProcessNodePosition(info);
                break;
            case PropertyValueUpdated_ChangeInfo info:
                ProcessNodePropertyValueUpdated(info);
                break;
            case NodeName_ChangeInfo info:
                ProcessNodeName(info);
                break;
            case FrameRate_ChangeInfo info:
                ProcessFrameRate(info);
                break;
            case SetOnionFrames_PassthroughAction info:
                ProcessSetOnionFrames(info);
                break;
        }
    }

    private void ProcessReferenceLayerIsVisible(ReferenceLayerIsVisible_ChangeInfo info)
    {
        doc.ReferenceLayerHandler.SetReferenceLayerIsVisible(info.IsVisible);
    }

    private void ProcessTransformReferenceLayer(TransformReferenceLayer_ChangeInfo info)
    {
        doc.ReferenceLayerHandler.TransformReferenceLayer(info.Corners);
    }

    private void ProcessDeleteReferenceLayer(DeleteReferenceLayer_ChangeInfo info)
    {
        doc.ReferenceLayerHandler.DeleteReferenceLayer();
    }

    private void ProcessSetReferenceLayer(SetReferenceLayer_ChangeInfo info)
    {
        doc.ReferenceLayerHandler.SetReferenceLayer(info.ImagePbgra8888Bytes, info.ImageSize, info.Shape);
    }
    
    private void ProcessReferenceLayerTopMost(ReferenceLayerTopMost_ChangeInfo info)
    {
        doc.ReferenceLayerHandler.SetReferenceLayerTopMost(info.IsTopMost);
    }

    private void ProcessRemoveSoftSelectedMember(RemoveSoftSelectedMember_PassthroughAction info)
    {
        IStructureMemberHandler? member = doc.StructureHelper.Find(info.Id);
        if (member is null || member.Selection == StructureMemberSelectionType.Hard)
            return;
        if (member.Selection != StructureMemberSelectionType.Soft)
            return;
        member.Selection = StructureMemberSelectionType.None;
        // TODO: Make sure Selection raises property changed internally
        //member.OnPropertyChanged(nameof(member.Selection));
        doc.RemoveSoftSelectedMember(member);
    }

    private void ProcessClearSoftSelectedMembers(ClearSoftSelectedMembers_PassthroughAction info)
    {
        foreach (IStructureMemberHandler? oldMember in doc.SoftSelectedStructureMembers)
        {
            if (oldMember.Selection == StructureMemberSelectionType.Hard)
                continue;
            oldMember.Selection = StructureMemberSelectionType.None;
            //oldMember.OnPropertyChanged(nameof(oldMember.Selection));
        }
        doc.ClearSoftSelectedMembers();
    }

    private void ProcessAddSoftSelectedMember(AddSoftSelectedMember_PassthroughAction info)
    {
        IStructureMemberHandler? member = doc.StructureHelper.Find(info.Id);
        if (member is null || member.Selection == StructureMemberSelectionType.Hard)
            return;
        member.Selection = StructureMemberSelectionType.Soft;
        //member.OnPropertyChanged(nameof(member.Selection));
        doc.AddSoftSelectedMember(member);
    }

    private void ProcessSetSelectedMember(SetSelectedMember_PassthroughAction info)
    {
        IStructureMemberHandler? member = doc.StructureHelper.Find(info.Id);
        if (member is null || member.Selection == StructureMemberSelectionType.Hard)
            return;
        
        if (doc.SelectedStructureMember is { } oldMember)
        {
            oldMember.Selection = StructureMemberSelectionType.None;
            //oldMember.OnPropertyChanged(nameof(oldMember.Selection));
        }
        member.Selection = StructureMemberSelectionType.Hard;
        //member.OnPropertyChanged(nameof(member.Selection));
        doc.SetSelectedMember(member);
    }

    private void ProcessMaskIsVisible(StructureMemberMaskIsVisible_ChangeInfo info)
    {
        IStructureMemberHandler? member = doc.StructureHelper.FindOrThrow(info.Id);
        member.SetMaskIsVisible(info.IsVisible);
    }

    private void ProcessClipToMemberBelow(StructureMemberClipToMemberBelow_ChangeInfo info)
    {
        IStructureMemberHandler? member = doc.StructureHelper.FindOrThrow(info.Id);
        member.SetClipToMemberBelowEnabled(info.ClipToMemberBelow);
    }

    private void ProcessSymmetryPosition(SymmetryAxisPosition_ChangeInfo info)
    {
        if (info.Direction == SymmetryAxisDirection.Horizontal)
            doc.SetHorizontalSymmetryAxisY(info.NewPosition);
        else if (info.Direction == SymmetryAxisDirection.Vertical)
            doc.SetVerticalSymmetryAxisX(info.NewPosition);
    }

    private void ProcessSymmetryState(SymmetryAxisState_ChangeInfo info)
    {
        if (info.Direction == SymmetryAxisDirection.Horizontal)
            doc.SetHorizontalSymmetryAxisEnabled(info.State);
        else if (info.Direction == SymmetryAxisDirection.Vertical)
            doc.SetVerticalSymmetryAxisEnabled(info.State);
    }

    private void ProcessSelection(Selection_ChangeInfo info)
    {
        doc.UpdateSelectionPath(info.NewPath);
    }

    private void ProcessLayerLockTransparency(LayerLockTransparency_ChangeInfo info)
    {
        ILayerHandler? layer = (ILayerHandler)doc.StructureHelper.FindOrThrow(info.Id);
        layer.SetLockTransparency(info.LockTransparency);
    }

    private void ProcessStructureMemberBlendMode(StructureMemberBlendMode_ChangeInfo info)
    {
        IStructureMemberHandler? memberVm = doc.StructureHelper.FindOrThrow(info.Id);
        memberVm.SetBlendMode(info.BlendMode);
    }

    private void ProcessStructureMemberMask(StructureMemberMask_ChangeInfo info)
    {
        IStructureMemberHandler? memberVm = doc.StructureHelper.FindOrThrow(info.Id);

        memberVm.SetHasMask(info.HasMask);
        // TODO: Make sure HasMask raises property changed internally
        //memberVm.OnPropertyChanged(nameof(memberVm.MaskPreviewBitmap));
        if (!info.HasMask && memberVm is ILayerHandler layer)
            layer.ShouldDrawOnMask = false;
    }

    private void ProcessRefreshViewport(RefreshViewport_PassthroughAction info)
    {
        helper.State.Viewports[info.Info.Id] = info.Info;
    }

    private void ProcessRemoveViewport(RemoveViewport_PassthroughAction info)
    {
        helper.State.Viewports.Remove(info.Id);
    }

    private void ProcessSize(Size_ChangeInfo info)
    {
        VecI oldSize = doc.SizeBindable;

        foreach ((ChunkResolution res, Texture surf) in doc.Surfaces)
        {
            surf.Dispose();
            VecI size = (VecI)(info.Size * res.Multiplier());
            doc.Surfaces[res] = new Texture(new VecI(Math.Max(size.X, 1), Math.Max(size.Y, 1))); //TODO: Bgra8888 was here
        }

        doc.SetSize(info.Size);
        doc.SetVerticalSymmetryAxisX(info.VerticalSymmetryAxisX);
        doc.SetHorizontalSymmetryAxisY(info.HorizontalSymmetryAxisY);

        VecI documentPreviewSize = StructureHelpers.CalculatePreviewSize(info.Size);
        doc.PreviewSurface.Dispose();
        doc.PreviewSurface = new Texture(documentPreviewSize); //TODO: Bgra8888 was here

        // TODO: Make sure property changed events are raised internally
        // UPDATE: I think I did, but I'll leave it commented out for now
        /*doc.OnPropertyChanged(nameof(doc.LazyBitmaps));
        doc.OnPropertyChanged(nameof(doc.PreviewBitmap));
        doc.InternalRaiseSizeChanged(new DocumentSizeChangedEventArgs(doc, oldSize, info.Size));*/
    }

    private void ProcessCreateStructureMember(CreateStructureMember_ChangeInfo info)
    {
        IStructureMemberHandler memberVM;
        if (info is CreateLayer_ChangeInfo layerInfo)
        {
            memberVM = doc.NodeGraphHandler.AllNodes.FirstOrDefault(x => x.Id == info.Id) as ILayerHandler;
            ((ILayerHandler)memberVM).SetLockTransparency(layerInfo.LockTransparency);
        }
        else if (info is CreateFolder_ChangeInfo)
        {
            memberVM = doc.NodeGraphHandler.AllNodes.FirstOrDefault(x => x.Id == info.Id) as IFolderHandler;
        }
        else
        {
            throw new NotSupportedException();
        }

        memberVM.SetOpacity(info.Opacity);
        memberVM.SetIsVisible(info.IsVisible);
        memberVM.SetClipToMemberBelowEnabled(info.ClipToMemberBelow);
        memberVM.SetName(info.Name);
        memberVM.SetHasMask(info.HasMask);
        memberVM.SetMaskIsVisible(info.MaskIsVisible);
        memberVM.SetBlendMode(info.BlendMode);
        
        //parentFolderVM.Children.Insert(info.Index, memberVM);

        /*if (info is CreateFolder_ChangeInfo folderInfo)
        {
            foreach (CreateStructureMember_ChangeInfo childInfo in folderInfo.Children)
            {
                ProcessCreateStructureMember(childInfo);
            }
        }
        */

        if (doc.SelectedStructureMember is not null)
        {
            doc.SelectedStructureMember.Selection = StructureMemberSelectionType.None;
            // TODO: Make sure property changed events are raised internally
            //doc.SelectedStructureMember.OnPropertyChanged(nameof(doc.SelectedStructureMember.Selection));
        }

        doc.SetSelectedMember(memberVM);
        memberVM.Selection = StructureMemberSelectionType.Hard;

        // TODO: Make sure property changed events are raised internally
        /*doc.OnPropertyChanged(nameof(doc.SelectedStructureMember));
        doc.OnPropertyChanged(nameof(memberVM.Selection));*/

        //doc.InternalRaiseLayersChanged(new LayersChangedEventArgs(info.Id, LayerAction.Add));
    }

    private void ProcessDeleteStructureMember(DeleteStructureMember_ChangeInfo info)
    {
        IStructureMemberHandler memberVM = doc.StructureHelper.Find(info.Id);
        //folderVM.Children.Remove(memberVM);
        if (doc.SelectedStructureMember == memberVM)
            doc.SetSelectedMember(null);
        doc.ClearSoftSelectedMembers();
        // TODO: Make sure property changed events are raised internally
        //doc.InternalRaiseLayersChanged(new LayersChangedEventArgs(info.Id, LayerAction.Remove));
    }

    private void ProcessUpdateStructureMemberIsVisible(StructureMemberIsVisible_ChangeInfo info)
    {
        IStructureMemberHandler? memberVM = doc.StructureHelper.FindOrThrow(info.Id);
        memberVM.SetIsVisible(info.IsVisible);
    }

    private void ProcessUpdateStructureMemberName(StructureMemberName_ChangeInfo info)
    {
        IStructureMemberHandler? memberVM = doc.StructureHelper.FindOrThrow(info.Id);
        memberVM.SetName(info.Name);
    }

    private void ProcessUpdateStructureMemberOpacity(StructureMemberOpacity_ChangeInfo info)
    {
        IStructureMemberHandler? memberVM = doc.StructureHelper.FindOrThrow(info.Id);
        memberVM.SetOpacity(info.Opacity);
    }

    private void ProcessMoveStructureMember(MoveStructureMember_ChangeInfo info)
    {
         // TODO: uh why is this empty, find out why
    }
    
    private void ProcessToggleOnionSkinning(ToggleOnionSkinning_PassthroughAction info)
    {
        doc.AnimationHandler.SetOnionSkinning(info.IsOnionSkinningEnabled);
    }
    
    private void ProcessCreateRasterKeyFrame(CreateRasterKeyFrame_ChangeInfo info)
    {
        doc.AnimationHandler.AddKeyFrame(new RasterKeyFrameViewModel(info.TargetLayerGuid, info.Frame, 1, info.KeyFrameId, 
            (DocumentViewModel)doc, helper));
    }
    
    private void ProcessDeleteKeyFrame(DeleteKeyFrame_ChangeInfo info)
    {
        doc.AnimationHandler.RemoveKeyFrame(info.DeletedKeyFrameId);
    }
    
    private void ProcessActiveFrame(SetActiveFrame_PassthroughAction info)
    {
        doc.AnimationHandler.SetActiveFrame(info.Frame);
    }
    
    private void ProcessKeyFrameLength(KeyFrameLength_ChangeInfo info)
    {
        doc.AnimationHandler.SetFrameLength(info.KeyFrameGuid, info.StartFrame, info.Duration);
    }
    
    private void ProcessKeyFrameVisibility(KeyFrameVisibility_ChangeInfo info)
    {
        doc.AnimationHandler.SetKeyFrameVisibility(info.KeyFrameId, info.IsVisible);
    }
    
    private void ProcessAddSelectedKeyFrame(AddSelectedKeyFrame_PassthroughAction info)
    {
        doc.AnimationHandler.AddSelectedKeyFrame(info.KeyFrameGuid);
    }
    
    private void ProcessRemoveSelectedKeyFrame(RemoveSelectedKeyFrame_PassthroughAction info)
    {
        doc.AnimationHandler.RemoveSelectedKeyFrame(info.KeyFrameGuid);
    }
    
    private void ClearSelectedKeyFrames(ClearSelectedKeyFrames_PassthroughAction info)
    {
        doc.AnimationHandler.ClearSelectedKeyFrames();
    }
    
    private void ProcessCreateNode<T>(CreateNode_ChangeInfo info) where T : NodeViewModel, new()
    {
        T node = new T()
        {
            InternalName = info.InternalName,
            Id = info.Id,
            Document = (DocumentViewModel)doc,
            Internals = helper
        };

        node.SetName(info.NodeName);
        node.SetPosition(info.Position);
        
        List<INodePropertyHandler> inputs = CreateProperties(info.Inputs, node, true);
        List<INodePropertyHandler> outputs = CreateProperties(info.Outputs, node, false);
        node.Inputs.AddRange(inputs);
        node.Outputs.AddRange(outputs);
        doc.NodeGraphHandler.AddNode(node);
    }
    
    private List<INodePropertyHandler> CreateProperties(ImmutableArray<NodePropertyInfo> source, NodeViewModel node, bool isInput)
    {
        List<INodePropertyHandler> inputs = new();
        foreach (var input in source)
        {
            var prop = NodePropertyViewModel.CreateFromType(input.ValueType, node);
            prop.DisplayName = input.DisplayName;
            prop.PropertyName = input.PropertyName;
            prop.IsInput = isInput;
            prop.IsFunc = input.ValueType.IsAssignableTo(typeof(Delegate));
            prop.InternalSetValue(input.InputValue);
            inputs.Add(prop);
        }
        
        return inputs;
    }
    
    private void ProcessDeleteNode(DeleteNode_ChangeInfo info)
    {
        doc.NodeGraphHandler.RemoveConnections(info.Id);
        doc.NodeGraphHandler.RemoveNode(info.Id);
    }
    
    private void ProcessCreateNodeFrame(CreateNodeFrame_ChangeInfo info)
    {
        doc.NodeGraphHandler.AddFrame(info.Id, info.NodeIds);
    }

    private void ProcessCreateNodeZone(CreateNodeZone_ChangeInfo info)
    {
        doc.NodeGraphHandler.AddZone(info.Id, info.internalName, info.StartId, info.EndId);
    }

    private void ProcessDeleteNodeFrame(DeleteNodeFrame_ChangeInfo info)
    {
        doc.NodeGraphHandler.RemoveFrame(info.Id);
    }

    private void ProcessConnectProperty(ConnectProperty_ChangeInfo info)
    {
        NodeViewModel outputNode = info.OutputNodeId.HasValue ? doc.StructureHelper.FindNode<NodeViewModel>(info.OutputNodeId.Value) : null;
        NodeViewModel inputNode = doc.StructureHelper.FindNode<NodeViewModel>(info.InputNodeId);

        if (inputNode != null && outputNode != null)
        {
            NodeConnectionViewModel connection = new NodeConnectionViewModel()
            {
                InputNode = inputNode,
                OutputNode = outputNode,
                InputProperty = inputNode.FindInputProperty(info.InputProperty),
                OutputProperty = outputNode.FindOutputProperty(info.OutputProperty)
            };
            
            doc.NodeGraphHandler.SetConnection(connection);
        }
        else if(info.OutputProperty == null)
        {
            doc.NodeGraphHandler.RemoveConnection(info.InputNodeId, info.InputProperty);
        }
        else
        {
#if DEBUG
            throw new MissingNodeException("Connection requested for a node that doesn't exist");
#endif
        }
    }
    
    private void ProcessNodePosition(NodePosition_ChangeInfo info)
    {
        NodeViewModel node = doc.StructureHelper.FindNode<NodeViewModel>(info.NodeId);
        node.SetPosition(info.NewPosition);
    }
    
    private void ProcessNodePropertyValueUpdated(PropertyValueUpdated_ChangeInfo info)
    {
        NodeViewModel node = doc.StructureHelper.FindNode<NodeViewModel>(info.NodeId);
        var property = node.FindInputProperty(info.Property);
        
        property.InternalSetValue(info.Value);
    }
    
    private void ProcessNodeName(NodeName_ChangeInfo info)
    {
        NodeViewModel node = doc.StructureHelper.FindNode<NodeViewModel>(info.NodeId);
        node.SetName(info.NewName);
    }
    
    private void ProcessFrameRate(FrameRate_ChangeInfo info)
    {
        doc.AnimationHandler.SetFrameRate(info.NewFrameRate);
    }
    
    private void ProcessSetOnionFrames(SetOnionFrames_PassthroughAction info)
    {
        doc.AnimationHandler.SetOnionFrames(info.Frames);
    }
}
