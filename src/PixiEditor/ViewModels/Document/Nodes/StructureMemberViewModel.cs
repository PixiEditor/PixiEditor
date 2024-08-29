using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.DrawingApi.Core;
using PixiEditor.Helpers;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Layers;
using PixiEditor.Numerics;
using PixiEditor.ViewModels.Nodes;
using BlendMode = PixiEditor.ChangeableDocument.Enums.BlendMode;

namespace PixiEditor.ViewModels.Document.Nodes;
#nullable enable
internal abstract class StructureMemberViewModel<T> : NodeViewModel<T>, IStructureMemberHandler where T : Node
{
    public StructureMemberViewModel()
    {
        
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
            if (!Document.UpdateableChangeActive)
                Internals.ActionAccumulator.AddFinishedActions(new StructureMemberIsVisible_Action(value, Id));
        }
    }

    private bool maskIsVisible;

    public RectI? TightBounds => Internals.Tracker.Document.FindMember(Id)
        ?.GetTightBounds(Document.AnimationDataViewModel.ActiveFrameBindable);

    public void SetMaskIsVisible(bool maskIsVisible)
    {
        this.maskIsVisible = maskIsVisible;
        OnPropertyChanged(nameof(MaskIsVisibleBindable));
    }

    public bool MaskIsVisibleBindable
    {
        get => maskIsVisible;
        set
        {
            if (!Document.UpdateableChangeActive)
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
            if (!Document.UpdateableChangeActive)
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
            if (!Document.UpdateableChangeActive)
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
            if (Document.UpdateableChangeActive)
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

    private Texture? previewSurface;
    private Texture? maskPreviewSurface;

    public Texture? PreviewSurface
    {
        get => previewSurface;
        set => SetProperty(ref previewSurface, value);
    }

    public Texture? MaskPreviewSurface
    {
        get => maskPreviewSurface;
        set => SetProperty(ref maskPreviewSurface, value);
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
