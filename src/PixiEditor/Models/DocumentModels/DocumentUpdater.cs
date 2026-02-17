using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using Avalonia.Threading;
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
using Drawie.Backend.Core;
using Drawie.Backend.Core.Shaders.Generation;
using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DocumentModels.Public;
using PixiEditor.Models.DocumentPassthroughActions;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Layers;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Workspace;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph.Blackboard;
using PixiEditor.Models.BrushEngine;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.DocumentModels.UpdateableChangeExecutors.Features;
using PixiEditor.ViewModels;
using PixiEditor.ViewModels.Document;
using PixiEditor.ViewModels.Document.Blackboard;
using PixiEditor.ViewModels.Document.Nodes;
using PixiEditor.ViewModels.Document.Nodes.Brushes;
using PixiEditor.ViewModels.Nodes;
using PixiEditor.ViewModels.SubViewModels;

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
            case ChangeError_Info error:
                ProcessError(error);
                break;
            case InvokeAction_PassthroughAction info:
                ProcessInvokeAction(info);
                break;
            case CreateStructureMember_ChangeInfo info:
                ProcessCreateNode(info);
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
                ProcessUpdateStructureMemberIsVisible(info.Id, info.IsVisible);
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
                ProcessCreateNode(info);
                ProcessCreateBrushNodeIfNeeded(info);
                break;
            case DeleteNode_ChangeInfo info:
                ProcessDeleteNode(info);
                ProcessDeleteBrushNodeIfNeeded(info);
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
            case NodeInputsChanged_ChangeInfo info:
                ProcessInputsChanged(info);
                break;
            case NodeOutputsChanged_ChangeInfo info:
                ProcessOutputsChanged(info);
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
            case OnionFrames_ChangeInfo info:
                ProcessSetOnionFrames(info);
                break;
            case SetPlayingState_PassthroughAction info:
                ProcessPlayAnimation(info);
                break;
            case ProcessingColorSpace_ChangeInfo info:
                ProcessProcessingColorSpace(info);
                break;
            case MarkAsAutosaved_PassthroughAction info:
                MarkAsAutosaved(info);
                break;
            case ComputedPropertyValue_ChangeInfo info:
                ProcessComputedPropertyValue(info);
                break;
            case DefaultEndFrame_ChangeInfo info:
                ProcessNewDefaultEndFrame(info);
                break;
            case BlackboardVariable_ChangeInfo info:
                ProcessBlackboardVariable(info);
                break;
            case RenameBlackboardVariable_ChangeInfo info:
                ProcessRenameBlackboardVariable(info);
                break;
            case BlackboardVariableRemoved_ChangeInfo info:
                ProcessRemoveBlackboardVariable(info);
                break;
            case NestedDocumentLink_ChangeInfo info:
                ProcessNestedDocumentLinkChangeInfo(info);
                break;
            case BlackboardVariableExposed_ChangeInfo info:
                ProcessBlackboardVariableExposedChangeInfo(info);
                break;
            case FallbackAnimationToLayerImage_ChangeInfo info:
                ProcessFallbackAnimationToLayerImage(info);
                break;
        }
    }

    private void ProcessError(ChangeError_Info info)
    {
        Dispatcher.UIThread.Post(() =>
        {
            NoticeDialog.Show(info.Message, "ERROR");
        });
    }

    private void ProcessInvokeAction(InvokeAction_PassthroughAction info)
    {
        info.Action.Invoke();
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
        doc.RemoveSoftSelectedMember(member);
    }

    private void ProcessClearSoftSelectedMembers(ClearSoftSelectedMembers_PassthroughAction info)
    {
        foreach (IStructureMemberHandler? oldMember in doc.SoftSelectedStructureMembers)
        {
            if (oldMember.Selection == StructureMemberSelectionType.Hard)
                continue;
            oldMember.Selection = StructureMemberSelectionType.None;
        }

        doc.ClearSoftSelectedMembers();
    }

    private void ProcessAddSoftSelectedMember(AddSoftSelectedMember_PassthroughAction info)
    {
        IStructureMemberHandler? member = doc.StructureHelper.Find(info.Id);
        if (member is null || member.Selection == StructureMemberSelectionType.Hard)
            return;
        member.Selection = StructureMemberSelectionType.Soft;
        doc.AddSoftSelectedMember(member);
    }

    private void ProcessSetSelectedMember(SetSelectedMember_PassthroughAction info)
    {
        IStructureMemberHandler? member = doc.StructureHelper.Find(info.Id);
        if (member is not null && member.Selection == StructureMemberSelectionType.Hard)
            return;

        if (doc.SelectedStructureMember is { } oldMember)
        {
            oldMember.Selection = StructureMemberSelectionType.None;
        }

        if (member != null)
        {
            member.Selection = StructureMemberSelectionType.Hard;
        }

        doc.SetSelectedMember(member);
    }

    private void ProcessMaskIsVisible(StructureMemberMaskIsVisible_ChangeInfo info)
    {
        IStructureMemberHandler? member = doc.StructureHelper.FindOrThrow(info.Id);
        member.SetMaskIsVisible(info.IsVisible);

        if (member.InputPropertyMap.TryGetValue(StructureNode.MaskIsVisiblePropertyName, out var propHandler))
        {
            propHandler.InternalSetValue(info.IsVisible);
        }
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
        if (layer is ITransparencyLockableMember transparencyLockableLayer)
            transparencyLockableLayer.SetLockTransparency(info.LockTransparency);
    }

    private void ProcessStructureMemberBlendMode(StructureMemberBlendMode_ChangeInfo info)
    {
        IStructureMemberHandler? memberVm = doc.StructureHelper.FindOrThrow(info.Id);
        if (memberVm.InputPropertyMap.TryGetValue(StructureNode.BlendModePropertyName, out var propHandler))
        {
            propHandler.InternalSetValue(info.BlendMode);
        }

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
        doc.SetSize(info.Size);
        doc.SetVerticalSymmetryAxisX(info.VerticalSymmetryAxisX);
        doc.SetHorizontalSymmetryAxisY(info.HorizontalSymmetryAxisY);
    }

    private void ProcessCreateStructureMember(CreateStructureMember_ChangeInfo info)
    {
        IStructureMemberHandler memberVM;
        if (info is CreateLayer_ChangeInfo layerInfo)
        {
            memberVM = doc.NodeGraphHandler.NodeLookup.TryGetValue(layerInfo.Id, out var node)
                ? node as IStructureMemberHandler
                : null;
            if (memberVM is ITransparencyLockableMember transparencyLockableMember)
            {
                transparencyLockableMember.SetLockTransparency(layerInfo.LockTransparency);
            }
        }
        else if (info is CreateFolder_ChangeInfo)
        {
            memberVM = doc.NodeGraphHandler.NodeLookup.TryGetValue(info.Id, out var node)
                ? node as IFolderHandler
                : null;
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
        }

        doc.SetSelectedMember(memberVM);
        memberVM.Selection = StructureMemberSelectionType.Hard;

        doc.InternalRaiseLayersChanged(new LayersChangedEventArgs(info.Id, LayerAction.Add));
    }

    private void ProcessDeleteStructureMember(DeleteStructureMember_ChangeInfo info)
    {
        IStructureMemberHandler memberVM = doc.StructureHelper.Find(info.Id);
        if (doc.SelectedStructureMember == memberVM)
        {
            var closestId = doc.StructureHelper.FindClosestMember(new[] { info.Id });
            var closestMember = doc.StructureHelper.Find(closestId);

            if (closestMember == null)
            {
                closestMember = doc.NodeGraphHandler.StructureTree.Members.FirstOrDefault(x => x.Id != info.Id);
            }

            if (closestMember != null)
            {
                closestMember.Selection = StructureMemberSelectionType.Hard;
            }


            doc.SetSelectedMember(closestMember);
        }

        doc.ClearSoftSelectedMembers();
        doc.InternalRaiseLayersChanged(new LayersChangedEventArgs(info.Id, LayerAction.Remove));
    }

    private void ProcessUpdateStructureMemberIsVisible(Guid id, bool isVisible)
    {
        IStructureMemberHandler? memberVM = doc.StructureHelper.FindOrThrow(id);
        memberVM.SetIsVisible(isVisible);
        if (memberVM.InputPropertyMap.TryGetValue(StructureNode.IsVisiblePropertyName, out var propHandler))
        {
            propHandler.InternalSetValue(isVisible);
        }

        UpdateMemberSnapping(memberVM);
    }

    private void UpdateMemberSnapping(IStructureMemberHandler memberVM)
    {
        List<IStructureMemberHandler>? children = null;
        if (memberVM is IFolderHandler folder)
        {
            children = doc.StructureHelper.GetFolderChildren(folder.Id);
        }

        bool isTransformingMember = helper.ChangeController.TryGetExecutorFeature<ITransformableExecutor>()?
            .IsTransformingMember(memberVM.Id) ?? false;
        if (memberVM.IsVisibleStructurally && !isTransformingMember)
        {
            doc.SnappingHandler.AddFromBounds(memberVM.Id.ToString(), () => memberVM.TightBounds ?? RectD.Empty);
        }
        else
        {
            doc.SnappingHandler.Remove(memberVM.Id.ToString());
        }

        if (children != null)
        {
            foreach (IStructureMemberHandler child in children)
            {
                isTransformingMember = helper.ChangeController.TryGetExecutorFeature<ITransformableExecutor>()?
                    .IsTransformingMember(child.Id) ?? false;

                if (child.IsVisibleStructurally && !isTransformingMember)
                {
                    doc.SnappingHandler.AddFromBounds(child.Id.ToString(),
                        () => child.TightBounds ?? RectD.Empty);
                }
                else
                {
                    doc.SnappingHandler.Remove(child.Id.ToString());
                }
            }
        }
    }

    private void ProcessUpdateStructureMemberName(StructureMemberName_ChangeInfo info)
    {
        IStructureMemberHandler? memberVM = doc.StructureHelper.FindOrThrow(info.Id);
        memberVM.SetName(info.Name);
    }

    private void ProcessUpdateStructureMemberOpacity(StructureMemberOpacity_ChangeInfo info)
    {
        IStructureMemberHandler? memberVM = doc.StructureHelper.FindOrThrow(info.Id);
        if (memberVM.InputPropertyMap.TryGetValue(StructureNode.OpacityPropertyName, out var propHandler))
        {
            propHandler.InternalSetValue(info.Opacity);
        }

        memberVM.SetOpacity(info.Opacity);
    }

    private void ProcessMoveStructureMember(MoveStructureMember_ChangeInfo info)
    {
        doc.InternalRaiseLayersChanged(new LayersChangedEventArgs(info.Id, LayerAction.Move));
    }

    private void ProcessToggleOnionSkinning(ToggleOnionSkinning_PassthroughAction info)
    {
        doc.AnimationHandler.SetOnionSkinning(info.IsOnionSkinningEnabled);
    }

    private void ProcessPlayAnimation(SetPlayingState_PassthroughAction info)
    {
        doc.AnimationHandler.SetPlayingState(info.Play);
    }

    private void ProcessCreateRasterKeyFrame(CreateRasterKeyFrame_ChangeInfo info)
    {
        var vm = new RasterCelViewModel(info.TargetLayerGuid, info.Frame, 1,
            info.KeyFrameId,
            (DocumentViewModel)doc, helper);

        doc.AnimationHandler.AddKeyFrame(vm);
        doc.InternalRaiseKeyFrameCreated(vm);
    }

    private void ProcessDeleteKeyFrame(DeleteKeyFrame_ChangeInfo info)
    {
        doc.AnimationHandler.RemoveKeyFrame(info.DeletedKeyFrameId);
    }

    private void ProcessActiveFrame(SetActiveFrame_PassthroughAction info)
    {
        doc.AnimationHandler.SetActiveFrame(info.Frame);
    }

    private void ProcessNewDefaultEndFrame(DefaultEndFrame_ChangeInfo info)
    {
        doc.AnimationHandler.SetDefaultEndFrame(info.NewDefaultEndFrame);
    }

    private void ProcessKeyFrameLength(KeyFrameLength_ChangeInfo info)
    {
        doc.AnimationHandler.SetCelLength(info.KeyFrameGuid, info.StartFrame, info.Duration);
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

    private void ProcessCreateNode(CreateNode_ChangeInfo info)
    {
        var nodeType = info.Metadata.NodeType;

        var ns = nodeType.Namespace.Replace("ChangeableDocument.Changeables.Graph.", "ViewModels.Document.");
        var name = nodeType.Name.Replace("Node", "NodeViewModel");
        var fullViewModelName = $"{ns}.{name}";
        var nodeViewModelType = Type.GetType(fullViewModelName);

        if (nodeViewModelType == null)
            throw new NullReferenceException($"No ViewModel found for {nodeType}. Looking for '{fullViewModelName}'");

        var viewModel = (NodeViewModel)Activator.CreateInstance(nodeViewModelType);

        InitializeNodeViewModel(info, viewModel);
    }

    private void ProcessInputsChanged(NodeInputsChanged_ChangeInfo info)
    {
        NodeViewModel node = doc.StructureHelper.FindNode<NodeViewModel>(info.NodeId);

        List<INodePropertyHandler> removedInputs = new List<INodePropertyHandler>();

        foreach (var input in node.Inputs)
        {
            if (!info.Inputs.Any(x => x.PropertyName == input.PropertyName))
            {
                removedInputs.Add(input);
            }

            if (info.Inputs.FirstOrDefault(x =>
                    x.PropertyName == input.PropertyName && x.ValueType != input.PropertyType) is { } changedInput)
            {
                removedInputs.Add(input);
            }
        }

        foreach (var input in removedInputs)
        {
            node.Inputs.Remove(input);
            doc.NodeGraphHandler.RemoveConnection(input.Node.Id, input.PropertyName);
        }

        List<NodePropertyInfo> newInputs =
            info.Inputs.Where(x => node.Inputs.All(y => y.PropertyName != x.PropertyName)).ToList();

        List<INodePropertyHandler> inputs = CreateProperties([..newInputs], node, true);
        node.Inputs.AddRange(inputs);
    }

    private void ProcessOutputsChanged(NodeOutputsChanged_ChangeInfo info)
    {
        NodeViewModel node = doc.StructureHelper.FindNode<NodeViewModel>(info.NodeId);

        if (node == null)
        {
            return;
        }

        List<INodePropertyHandler> removedOutputs = new List<INodePropertyHandler>();

        foreach (var output in node.Outputs)
        {
            if (!info.Outputs.Any(x => x.PropertyName == output.PropertyName))
            {
                removedOutputs.Add(output);
            }

            if (info.Outputs.FirstOrDefault(x =>
                    x.PropertyName == output.PropertyName
                    && x.ValueType != output.Value?.GetType() && x.ValueType != output.PropertyType) is
                { } changedOutput)
            {
                removedOutputs.Add(output);
            }
        }

        foreach (var output in removedOutputs)
        {
            node.Outputs.Remove(output);
            doc.NodeGraphHandler.RemoveConnection(output.Node.Id, output.PropertyName);
        }

        List<NodePropertyInfo> newOutputs =
            info.Outputs.Where(x => node.Outputs.All(y => y.PropertyName != x.PropertyName)).ToList();

        List<INodePropertyHandler> outputs = CreateProperties([..newOutputs], node, false);
        node.Outputs.AddRange(outputs);
    }

    private void InitializeNodeViewModel(CreateNode_ChangeInfo info, NodeViewModel viewModel)
    {
        viewModel.Initialize(info.Id, info.InternalName, (DocumentViewModel)doc, helper);

        viewModel.SetName(info.NodeName);
        viewModel.SetPosition(info.Position);

        var inputs = CreateProperties(info.Inputs, viewModel, true);
        var outputs = CreateProperties(info.Outputs, viewModel, false);
        viewModel.Inputs.AddRange(inputs);
        viewModel.Outputs.AddRange(outputs);
        doc.NodeGraphHandler.AddNode(viewModel);

        viewModel.Metadata = info.Metadata;

        AddZoneIfNeeded(info, viewModel);
        LinkNestedDocumentIfNeeded(info);

        viewModel.OnInitialized();
    }

    private void AddZoneIfNeeded(CreateNode_ChangeInfo info, NodeViewModel node)
    {
        if (node.Metadata?.PairNodeGuid != null)
        {
            if (node.Metadata.PairNodeGuid == Guid.Empty) return;

            INodeHandler otherNode =
                doc.NodeGraphHandler.AllNodes.FirstOrDefault(x => x.Id == node.Metadata.PairNodeGuid);
            if (otherNode != null)
            {
                bool zoneExists =
                    doc.NodeGraphHandler.Frames.Any(x => x is NodeZoneViewModel zone && zone.Nodes.Contains(node));

                if (!zoneExists)
                {
                    doc.NodeGraphHandler.AddZone(Guid.NewGuid(), $"PixiEditor.{info.Metadata.ZoneUniqueName}", node.Id,
                        node.Metadata.PairNodeGuid.Value);
                }
            }
        }
    }

    private List<INodePropertyHandler> CreateProperties(ImmutableArray<NodePropertyInfo> source, NodeViewModel node,
        bool isInput)
    {
        List<INodePropertyHandler> inputs = new();
        foreach (var propInfo in source)
        {
            var prop = NodePropertyViewModel.CreateFromType(propInfo.ValueType, node);
            prop.DisplayName = propInfo.DisplayName;
            prop.PropertyName = propInfo.PropertyName;
            prop.IsInput = isInput;
            prop.IsFunc = propInfo.ValueType.IsAssignableTo(typeof(Delegate));
            prop.InternalSetValue(prop.IsFunc
                ? (propInfo.InputValue as ShaderExpressionVariable)?.GetConstant()
                : propInfo.InputValue);
            inputs.Add(prop);
            foreach (var propInfoConnectedProperty in propInfo.ConnectedProperties)
            {
                doc.NodeGraphHandler.SetConnection(new NodeConnectionViewModel()
                {
                    InputNode =
                        isInput ? node : doc.StructureHelper.FindNode<NodeViewModel>(propInfoConnectedProperty.NodeId),
                    OutputNode =
                        isInput ? doc.StructureHelper.FindNode<NodeViewModel>(propInfoConnectedProperty.NodeId) : node,
                    InputProperty = isInput
                        ? prop
                        : doc.StructureHelper.FindNode<NodeViewModel>(propInfoConnectedProperty.NodeId)
                            .FindInputProperty(propInfoConnectedProperty.PropertyName),
                    OutputProperty = isInput
                        ? doc.StructureHelper.FindNode<NodeViewModel>(propInfoConnectedProperty.NodeId)
                            .FindOutputProperty(propInfoConnectedProperty.PropertyName)
                        : prop
                });
            }
        }

        return inputs;
    }

    private void ProcessDeleteNode(DeleteNode_ChangeInfo info)
    {
        foreach (var frame in doc.NodeGraphHandler.Frames)
        {
            if (frame is NodeZoneViewModel zone)
            {
                if (zone.Nodes.Any(x => x.Id == info.Id))
                {
                    doc.NodeGraphHandler.RemoveFrame(zone.Id);
                    break;
                }
            }
        }

        doc.NodeGraphHandler.RemoveConnections(info.Id);
        doc.NodeGraphHandler.RemoveNode(info.Id);

        doc.SnappingHandler.SnappingController.RemoveAll(info.Id.ToString());
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
        NodeViewModel outputNode = info.OutputNodeId.HasValue
            ? doc.StructureHelper.FindNode<NodeViewModel>(info.OutputNodeId.Value)
            : null;
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
        else if (info.OutputProperty == null)
        {
            doc.NodeGraphHandler.RemoveConnection(info.InputNodeId, info.InputProperty);
        }
        else
        {
#if DEBUG
            throw new MissingNodeException("Connection requested for a node that doesn't exist");
#endif
        }

        if (inputNode is IStructureMemberHandler structureMember)
        {
            UpdateMemberSnapping(structureMember);
        }

        if (outputNode is IStructureMemberHandler structureMember2)
        {
            UpdateMemberSnapping(structureMember2);
        }
    }

    private void ProcessNodePosition(NodePosition_ChangeInfo info)
    {
        NodeViewModel node = doc.StructureHelper.FindNode<NodeViewModel>(info.NodeId);
        if (node == null)
            return;

        node.SetPosition(info.NewPosition);
    }

    private void ProcessNodePropertyValueUpdated(PropertyValueUpdated_ChangeInfo info)
    {
        NodeViewModel node = doc.StructureHelper.FindNode<NodeViewModel>(info.NodeId);
        var property = node.FindInputProperty(info.Property);

        if (property == null)
            return;

        property.Errors = info.Errors;

        ProcessStructureMemberProperty(info, property);
        var toSet = property.IsFunc
            ? (info.Value as ShaderExpressionVariable)?.GetConstant() ?? info.Value
            : info.Value;
        property.InternalSetValue(toSet);

        if (info.Property == CustomOutputNode.OutputNamePropertyName)
        {
            doc.NodeGraphHandler.UpdateAvailableRenderOutputs();
        }

        if (info.Property == BrushOutputNode.BrushNameProperty)
        {
            var brush = ViewModelMain.Current.BrushesSubViewModel.BrushLibrary.Brushes.FirstOrDefault(x =>
                x.Key == node.Id && x.Value.Document.Id == doc.Id);
            if (brush.Value != null)
            {
                brush.Value.Name = info.Value?.ToString() ?? "Unnamed";
            }
        }

        if (info.Property == NestedDocumentNode.DocumentPropertyName)
        {
            string? path = null;
            Guid referenceId = Guid.Empty;
            if (info.Value is DocumentReference doc)
            {
                path = doc.OriginalFilePath;
                referenceId = doc.ReferenceId;
            }

            ProcessNestedDocumentLinkChangeInfo(new NestedDocumentLink_ChangeInfo(info.NodeId, path, referenceId));
        }
    }

    private void ProcessStructureMemberProperty(PropertyValueUpdated_ChangeInfo info, INodePropertyHandler property)
    {
        // TODO: This most likely can be handled inside viewmodel itself
        if (property.Node is IStructureMemberHandler structureMemberHandler && info.Value != null)
        {
            if (info.Property == StructureNode.IsVisiblePropertyName)
            {
                ProcessUpdateStructureMemberIsVisible(structureMemberHandler.Id, (bool)info.Value);
            }
            else if (info.Property == StructureNode.OpacityPropertyName)
            {
                structureMemberHandler.SetOpacity((float)info.Value);
            }
            else if (info.Property == StructureNode.ClipToPreviousMemberPropertyName)
            {
                structureMemberHandler.SetClipToMemberBelowEnabled((bool)info.Value);
            }
            else if (info.Property == StructureNode.MaskIsVisiblePropertyName)
            {
                structureMemberHandler.SetMaskIsVisible((bool)info.Value);
            }
            else if (info.Property == StructureNode.BlendModePropertyName)
            {
                structureMemberHandler.SetBlendMode((BlendMode)info.Value);
            }
        }
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

    private void ProcessSetOnionFrames(OnionFrames_ChangeInfo info)
    {
        doc.AnimationHandler.SetOnionFrames(info.OnionFrames, info.Opacity);
    }

    private void ProcessProcessingColorSpace(ProcessingColorSpace_ChangeInfo info)
    {
        doc.SetProcessingColorSpace(info.NewColorSpace);
    }

    private void MarkAsAutosaved(MarkAsAutosaved_PassthroughAction info)
    {
        doc.InternalMarkSaveState(info.Type);
    }

    private void ProcessComputedPropertyValue(ComputedPropertyValue_ChangeInfo info)
    {
        object finalValue = info.Value;
        // TODO: Why to string???
        /*if (info.Value != null && !info.Value.GetType().IsValueType && info.Value is not string)
        {
            bool valueToStringIsDefault = info.Value.GetType().FullName == info.Value.ToString();
            if (valueToStringIsDefault)
            {
                finalValue = info.Value?.GetType().Name ?? finalValue;
            }
        }*/

        NodeViewModel node = doc.StructureHelper.FindNode<NodeViewModel>(info.Node);
        INodePropertyHandler property;
        if (info.IsInput)
        {
            property = node.FindInputProperty(info.PropertyName);
        }
        else
        {
            property = node.FindOutputProperty(info.PropertyName);
        }

        if (property is null)
        {
            return;
        }

        property.InternalSetComputedValue(finalValue);
    }

    private void ProcessCreateBrushNodeIfNeeded(CreateNode_ChangeInfo info)
    {
        if (info.InternalName != "PixiEditor." + BrushOutputNode.NodeId) return;

        if (ViewModelMain.Current.DocumentManagerSubViewModel.Documents.All(x => x.Id != doc.Id)) return;

        string name = info.Inputs.FirstOrDefault(x => x.PropertyName == BrushOutputNode.BrushNameProperty)
            ?.InputValue?.ToString() ?? "Unnamed";

        doc.NodeGraphHandler.NodeLookup.TryGetValue(info.Id, out var node);
        if (node is BrushOutputNodeViewModel brushVm)
        {
            ViewModelMain.Current.BrushesSubViewModel.BrushLibrary.Add(
                new Brush(name, doc, "OPENED_DOCUMENT", null) { IsReadOnly = true, IsDuplicable = false });
        }
    }

    private void ProcessDeleteBrushNodeIfNeeded(DeleteNode_ChangeInfo info)
    {
        ViewModelMain.Current.BrushesSubViewModel.BrushLibrary.RemoveById(info.Id);
    }

    private void ProcessBlackboardVariable(BlackboardVariable_ChangeInfo info)
    {
        var existingVar = doc.NodeGraphHandler.Blackboard.GetVariable(info.Name);
        if (existingVar != null)
        {
            doc.NodeGraphHandler.Blackboard.SetVariableInternal(info.Name, info.Value);
            return;
        }

        doc.NodeGraphHandler.Blackboard.AddVariableInternal(info.Name, info.Type, info.Value, info.Unit, info.Min,
            info.Max);
    }

    private void ProcessRenameBlackboardVariable(RenameBlackboardVariable_ChangeInfo info)
    {
        doc.NodeGraphHandler.Blackboard.RenameVariableInternal(info.OldName, info.NewName);
    }

    private void ProcessRemoveBlackboardVariable(BlackboardVariableRemoved_ChangeInfo info)
    {
        doc.NodeGraphHandler.Blackboard.RemoveVariableInternal(info.VariableName);
    }

    private void LinkNestedDocumentIfNeeded(CreateNode_ChangeInfo info)
    {
        if (info.InternalName != "PixiEditor." + NestedDocumentNode.NodeId) return;

        var nestedDocInput = info.Inputs.FirstOrDefault(x => x.PropertyName == NestedDocumentNode.DocumentPropertyName);
        if (nestedDocInput?.InputValue is DocumentReference docRef)
        {
            ProcessNestedDocumentLinkChangeInfo(new NestedDocumentLink_ChangeInfo(info.Id,
                docRef.OriginalFilePath, docRef.ReferenceId));
        }
    }

    private void ProcessNestedDocumentLinkChangeInfo(NestedDocumentLink_ChangeInfo info)
    {
        var node = doc.StructureHelper.FindNode<NestedDocumentNodeViewModel>(info.NodeId);

        if (node.ReferenceId != info.ReferenceId)
        {
            ViewModelMain.Current.DocumentManagerSubViewModel.RemoveDocumentReferenceByNodeId(doc.Id, info.NodeId);
        }

        if (info.ReferenceId != Guid.Empty)
        {
            ViewModelMain.Current.DocumentManagerSubViewModel.AddDocumentReference(doc.Id, info.NodeId,
                info.OriginalFilePath, info.ReferenceId);
        }

        node.SetOriginalFilePath(info.OriginalFilePath);
        node.SetReferenceId(info.ReferenceId);
        node.UpdateLinkedStatus();
    }

    private void ProcessBlackboardVariableExposedChangeInfo(BlackboardVariableExposed_ChangeInfo info)
    {
        var existingVar = doc.NodeGraphHandler.Blackboard.GetVariable(info.VariableName);
        if (existingVar == null)
        {
            return;
        }

        if (existingVar is VariableViewModel varVm)
        {
            varVm.SetIsExposedInternal(info.Value);
        }
    }

    private void ProcessFallbackAnimationToLayerImage(FallbackAnimationToLayerImage_ChangeInfo info)
    {
        doc.AnimationHandler.SetFallbackAnimationToLayerImage(info.Value);
    }
}
