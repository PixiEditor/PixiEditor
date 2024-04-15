using Avalonia.Media.Imaging;
using ChunkyImageLib;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Models.DocumentModels;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.Models.Layers;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using BlendMode = PixiEditor.ChangeableDocument.Enums.BlendMode;

namespace PixiEditor.AvaloniaUI.ViewModels.Document;
#nullable enable
internal abstract class StructureMemberViewModel : ObservableObject, IStructureMemberHandler
{
    public DocumentViewModel Document { get; }
    protected DocumentInternalParts Internals { get; }


    private string name = "";
    public void SetName(string name)
    {
        this.name = name;
        OnPropertyChanged(nameof(NameBindable));
    }
    public string NameBindable
    {
        get => name;
        set
        {
            if (!Document.UpdateableChangeActive)
                Internals.ActionAccumulator.AddFinishedActions(new StructureMemberName_Action(GuidValue, value));
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
            if (!Document.UpdateableChangeActive)
                Internals.ActionAccumulator.AddFinishedActions(new StructureMemberIsVisible_Action(value, GuidValue));
        }
    }

    private bool maskIsVisible;
    public RectI? TightBounds => Internals.Tracker.Document.FindMember(GuidValue)?.GetTightBounds();

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
                Internals.ActionAccumulator.AddFinishedActions(new StructureMemberMaskIsVisible_Action(value, GuidValue));
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
                Internals.ActionAccumulator.AddFinishedActions(new StructureMemberBlendMode_Action(value, GuidValue));
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
                Internals.ActionAccumulator.AddFinishedActions(new StructureMemberClipToMemberBelow_Action(value, GuidValue));
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

    private Guid guidValue;
    public Guid GuidValue
    {
        get => guidValue;
    }

    private float opacity;

    public void SetOpacity(float opacity)
    {
        this.opacity = opacity;
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
                new StructureMemberOpacity_Action(GuidValue, newValue),
                new EndStructureMemberOpacity_Action());
        }
    }

    private StructureMemberSelectionType selection;

    public StructureMemberSelectionType Selection
    {
        get => selection;
        set => SetProperty(ref selection, value);
    }

    private Surface? previewSurface;
    private Surface? maskPreviewSurface;

    public Surface? PreviewSurface
    {
        get => previewSurface;
        set => SetProperty(ref previewSurface, value);
    }

    public Surface? MaskPreviewSurface
    {
        get => maskPreviewSurface;
        set => SetProperty(ref maskPreviewSurface, value);
    }

    IDocument IStructureMemberHandler.Document => Document;

    /// <summary>
    /// Calculates the size of a scaled-down preview for a given size of layer tight bounds.
    /// </summary>
    public static VecI CalculatePreviewSize(VecI tightBoundsSize)
    {
        double proportions = tightBoundsSize.Y / (double)tightBoundsSize.X;
        const int prSize = StructureHelpers.PreviewSize;
        return proportions > 1 ?
            new VecI(Math.Max((int)Math.Round(prSize / proportions), 1), prSize) :
            new VecI(prSize, Math.Max((int)Math.Round(prSize * proportions), 1));
    }

    public StructureMemberViewModel(DocumentViewModel doc, DocumentInternalParts internals, Guid guidValue)
    {
        Document = doc;
        Internals = internals;

        this.guidValue = guidValue;
        PreviewSurface = null;
    }
}
