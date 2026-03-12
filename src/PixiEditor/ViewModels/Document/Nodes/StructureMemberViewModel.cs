using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Numerics;
using PixiEditor.Helpers;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Rendering;
using Drawie.Numerics;
using PixiEditor.ViewModels.Nodes;
using BlendMode = PixiEditor.ChangeableDocument.Enums.BlendMode;

namespace PixiEditor.ViewModels.Document.Nodes;
#nullable enable
internal abstract class StructureMemberViewModel<T> : NodeViewModel<T>, IStructureMemberHandler where T : Node
{
    public StructureMemberViewModel()
    {
    }

    public override void OnInitialized()
    {
        var activeFrameProp = InputPropertyMap[StructureNode.CustomActiveFrameProperty];
        var activeNormalizedFrameProp = InputPropertyMap[StructureNode.CustomNormalizedTimeProperty];
        activeFrameProp.IsVisible = false;
        activeNormalizedFrameProp.IsVisible = false;

        if (InputPropertyMap.TryGetValue(StructureNode.UseCustomTimeProperty, out var useCustomTimeProp))
        {
            activeFrameProp.IsVisible = useCustomTimeProp.Value is true;
            activeNormalizedFrameProp.IsVisible = useCustomTimeProp.Value is true;
            useCustomTimeProp.ValueChanged += (s, e) =>
            {
                activeFrameProp.IsVisible = useCustomTimeProp.Value is true;
                activeNormalizedFrameProp.IsVisible = useCustomTimeProp.Value is true;
            };
        }
    }

    private bool isVisible;

    public void SetIsVisible(bool isVisible)
    {
        this.isVisible = isVisible;
        OnPropertyChanged(nameof(IsVisibleBindable));
    }

    public bool IsVisibleBindable
    {
        get => isVisible;
        set
        {
            if (!Document.BlockingUpdateableChangeActive)
                Internals.ActionAccumulator.AddFinishedActions(new StructureMemberIsVisible_Action(value, Id));
        }
    }

    private bool maskIsVisible;

    public RectD? TightBounds => Internals.Tracker.Document.FindMember(Id)
        ?.GetTightBounds(Document.AnimationDataViewModel.ActiveFrameBindable);

    public ShapeCorners TransformationCorners => Internals.Tracker.Document.FindMember(Id)
        ?.GetTransformationCorners(Document.AnimationDataViewModel.ActiveFrameBindable) ?? new ShapeCorners();

    public bool IsVisibleStructurally
    {
        get
        {
            if (!IsVisibleBindable)
                return false;

            bool visible = true;
            TraverseForwards((node, previous, output, input) =>
            {
                if (node is IFolderHandler parent && input is { PropertyName: FolderNode.ContentInternalName })
                {
                    visible = parent.IsVisibleBindable;
                    return visible ? Traverse.Further : Traverse.Exit;
                }

                return Traverse.Further;
            });

            return visible;
        }
    }

    public void SetMaskIsVisible(bool maskIsVisible)
    {
        this.maskIsVisible = maskIsVisible;
        OnPropertyChanged(nameof(MaskIsVisibleBindable));
    }

    private TexturePreview? maskPreview;
    private TexturePreview? preview;

    public TexturePreview? Preview
    {
        get => preview;
        set => SetProperty(ref preview, value);
    }

    public TexturePreview? MaskPreview
    {
        get => maskPreview;
        set => SetProperty(ref maskPreview, value);
    }

    public bool MaskIsVisibleBindable
    {
        get => maskIsVisible;
        set
        {
            if (!Document.BlockingUpdateableChangeActive)
                Internals.ActionAccumulator.AddFinishedActions(
                    new StructureMemberMaskIsVisible_Action(value, Id));
        }
    }

    private BlendMode blendMode;

    public void SetBlendMode(BlendMode blendMode)
    {
        this.blendMode = blendMode;
        OnPropertyChanged(nameof(BlendModeBindable));
    }

    public BlendMode BlendModeBindable
    {
        get => blendMode;
        set
        {
            if (!Document.BlockingUpdateableChangeActive)
                Internals.ActionAccumulator.AddFinishedActions(new StructureMemberBlendMode_Action(value, Id));
        }
    }

    private bool clipToMemberBelowEnabled;

    public void SetClipToMemberBelowEnabled(bool clipToMemberBelowEnabled)
    {
        this.clipToMemberBelowEnabled = clipToMemberBelowEnabled;
        OnPropertyChanged(nameof(ClipToMemberBelowEnabledBindable));
    }

    public bool ClipToMemberBelowEnabledBindable
    {
        get => clipToMemberBelowEnabled;
        set
        {
            if (!Document.BlockingUpdateableChangeActive)
                Internals.ActionAccumulator.AddFinishedActions(
                    new StructureMemberClipToMemberBelow_Action(value, Id));
        }
    }

    private bool hasMask;

    public void SetHasMask(bool hasMask)
    {
        this.hasMask = hasMask;
        OnPropertyChanged(nameof(HasMaskBindable));
    }

    public bool HasMaskBindable
    {
        get => hasMask;
    }

    private float opacity;

    public void SetOpacity(float newOpacity)
    {
        this.opacity = newOpacity;
        OnPropertyChanged(nameof(OpacityBindable));
    }

    public float OpacityBindable
    {
        get => opacity;
        set
        {
            if (Document.BlockingUpdateableChangeActive)
                return;
            float newValue = Math.Clamp(value, 0, 1);
            Internals.ActionAccumulator.AddFinishedActions(
                new StructureMemberOpacity_Action(Id, newValue),
                new EndStructureMemberOpacity_Action());
        }
    }

    private StructureMemberSelectionType selection;

    public StructureMemberSelectionType Selection
    {
        get => selection;
        set => SetProperty(ref selection, value);
    }

    IDocument IStructureMemberHandler.Document => Document;
}

public static class StructureMemberViewModel
{
    /// <summary>
    /// Calculates the size of a scaled-down preview for a given size of layer tight bounds.
    /// </summary>
    public static VecI CalculatePreviewSize(VecI tightBoundsSize)
    {
        double proportions = tightBoundsSize.Y / (double)tightBoundsSize.X;
        const int prSize = StructureHelpers.PreviewSize;
        return proportions > 1
            ? new VecI(Math.Max((int)Math.Round(prSize / proportions), 1), prSize)
            : new VecI(prSize, Math.Max((int)Math.Round(prSize * proportions), 1));
    }
}
