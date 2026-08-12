using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions.Generated;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Numerics;
using PixiEditor.Models.DocumentModels.UpdateableChangeExecutors.Features;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Tools;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ViewModels.Document.Nodes;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class PasteImageExecutor : UpdateableChangeExecutor, ITransformableExecutor, IMidChangeUndoableExecutor
{
    private readonly Surface image;
    private readonly VecI pos;
    private bool drawOnMask;
    private Guid? memberGuid;
    private IDisposable? restoreSnapping;

    public override bool BlocksOtherActions => false;

    public PasteImageExecutor(Surface image, VecI pos)
    {
        this.image = image;
        this.pos = pos;
    }

    public PasteImageExecutor(Surface image, VecI pos, Guid memberGuid, bool drawOnMask)
    {
        this.image = image;
        this.pos = pos;
        this.memberGuid = memberGuid;
        this.drawOnMask = drawOnMask;
    }

    public override ExecutionState Start()
    {
        if (memberGuid == null)
        {
            var member = document!.SelectedStructureMember;

            if (member is null)
                return ExecutionState.Error;
            drawOnMask = member is not ILayerHandler layer || layer.ShouldDrawOnMask;

            switch (drawOnMask)
            {
                case true when !member.HasMaskBindable:
                case false when member is not ILayerHandler:
                    return ExecutionState.Error;
            }

            memberGuid = member.Id;
        }

        ShapeCorners corners = new(new RectD(pos, image.Size));
        List<IAction> actions = new();
        if (NeedsCel())
        {
            actions.Add(new CreateCel_Action(memberGuid.Value, Guid.NewGuid(),
                document.AnimationHandler.ActiveFrameBindable, -1, Guid.Empty));
        }

        actions.Add(
            new ClearSelection_Action());
        actions.Add(
            new PasteImage_Action(image, corners, memberGuid.Value, false, drawOnMask,
                document.AnimationHandler.ActiveFrameBindable, default));

        internals!.ActionAccumulator.AddActions(actions.ToArray());

        document.TransformHandler.ShowTransform(DocumentTransformMode.Scale_Rotate_Shear_Perspective, true, corners,
            true);
        document.Operations.InvokeCustomAction(() =>
        {
            restoreSnapping = SimpleShapeToolExecutor.DisableSelfSnapping(memberGuid.Value, document);
        });

        return ExecutionState.Success;
    }

    public bool IsTransforming => true;

    public void OnTransformStarted()
    {

    }

    public void OnTransformChanged(ShapeCorners corners)
    {
        internals!.ActionAccumulator.AddActions(new PasteImage_Action(image, corners, memberGuid.Value, false,
            drawOnMask, document!.AnimationHandler.ActiveFrameBindable, default));
    }

    public void OnLineOverlayMoved(VecD start, VecD end) { }

    public void OnSelectedObjectNudged(VecI distance) => document!.TransformHandler.Nudge(distance);

    public bool IsTransformingMember(Guid id)
    {
        return id == memberGuid;
    }

    public void OnMidChangeUndo() => document!.TransformHandler.Undo();

    public void OnMidChangeRedo() => document!.TransformHandler.Redo();
    public bool CanUndo => document!.TransformHandler.HasUndo;
    public bool CanRedo => document!.TransformHandler.HasRedo;

    public void OnTransformApplied()
    {
        internals!.ActionAccumulator.AddFinishedActions(new EndPasteImage_Action());
        document!.TransformHandler.HideTransform();
        onEnded!.Invoke(this);
        restoreSnapping?.Dispose();
    }

    public override void ForceStop()
    {
        document!.TransformHandler.HideTransform();
        internals!.ActionAccumulator.AddFinishedActions(new EndPasteImage_Action());
        restoreSnapping?.Dispose();
    }

    private bool NeedsCel()
    {
        if (!TryGetAnimationGroup(out var animationGroupForLayer))
        {
            return false;
        }

        return animationGroupForLayer.IsVisible && !animationGroupForLayer.IsKeyFrameAt(document
            .AnimationHandler.ActiveFrameBindable);
    }

    private bool TryGetAnimationGroup(out ICelGroupHandler? animationGroupForLayer)
    {
        var activeDocument = document;

        if (activeDocument is null)
        {
            animationGroupForLayer = null;
            return false;
        }

        var selectedLayer = document.NodeGraphHandler.NodeLookup.GetValueOrDefault(memberGuid!.Value) as ILayerHandler;

        if (selectedLayer is not ImageLayerNodeViewModel rasterLayer)
        {
            animationGroupForLayer = null;
            return false;
        }

        animationGroupForLayer = document.AnimationHandler.KeyFrames
            .FirstOrDefault(x =>
                x.LayerGuid == rasterLayer.Id && x.Id == rasterLayer.Id) as ICelGroupHandler;

        if (animationGroupForLayer is null)
        {
            return false;
        }

        return true;
    }

    public bool IsFeatureEnabled<T>()
    {
        Type featureType = typeof(T);
        if (featureType == typeof(ITransformableExecutor))
            return IsTransforming;

        if (featureType == typeof(IMidChangeUndoableExecutor))
            return true;

        return false;
    }
}
