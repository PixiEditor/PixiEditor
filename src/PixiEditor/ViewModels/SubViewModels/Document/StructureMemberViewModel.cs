using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Enums;
using BlendMode = PixiEditor.ChangeableDocument.Enums.BlendMode;

namespace PixiEditor.ViewModels.SubViewModels.Document;
#nullable enable
internal abstract class StructureMemberViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public DocumentViewModel Document { get; }
    protected DocumentInternalParts Internals { get; }


    private string name = "";
    public void InternalSetName(string name)
    {
        this.name = name;
        RaisePropertyChanged(nameof(NameBindable));
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
    public void InternalSetIsVisible(bool isVisible)
    {
        this.isVisible = isVisible;
        RaisePropertyChanged(nameof(IsVisibleBindable));
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
    public void InternalSetMaskIsVisible(bool maskIsVisible)
    {
        this.maskIsVisible = maskIsVisible;
        RaisePropertyChanged(nameof(MaskIsVisibleBindable));
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
    public void InternalSetBlendMode(BlendMode blendMode)
    {
        this.blendMode = blendMode;
        RaisePropertyChanged(nameof(BlendModeBindable));
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
    public void InternalSetClipToMemberBelowEnabled(bool clipToMemberBelowEnabled)
    {
        this.clipToMemberBelowEnabled = clipToMemberBelowEnabled;
        RaisePropertyChanged(nameof(ClipToMemberBelowEnabledBindable));
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
    public void InternalSetHasMask(bool hasMask)
    {
        this.hasMask = hasMask;
        RaisePropertyChanged(nameof(HasMaskBindable));
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

    public void InternalSetOpacity(float opacity)
    {
        this.opacity = opacity;
        RaisePropertyChanged(nameof(OpacityBindable));
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

    public StructureMemberSelectionType Selection { get; set; }

    public const int PreviewSize = 48;
    public WriteableBitmap PreviewBitmap { get; set; }
    public DrawingSurface PreviewSurface { get; set; }

    public WriteableBitmap? MaskPreviewBitmap { get; set; }
    public DrawingSurface? MaskPreviewSurface { get; set; }

    public void RaisePropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>
    /// Calculates the size of a scaled-down preview for a given size of layer tight bounds.
    /// </summary>
    public static VecI CalculatePreviewSize(VecI tightBoundsSize)
    {
        double proportions = tightBoundsSize.Y / (double)tightBoundsSize.X;
        const int prSize = StructureMemberViewModel.PreviewSize;
        return proportions > 1 ?
            new VecI(Math.Max((int)Math.Round(prSize / proportions), 1), prSize) :
            new VecI(prSize, Math.Max((int)Math.Round(prSize * proportions), 1));
    }

    public static WriteableBitmap CreateBitmap(VecI size)
    {
        return new WriteableBitmap(Math.Max(size.X, 1), Math.Max(size.Y, 1), 96, 96, PixelFormats.Pbgra32, null);
    }

    public static DrawingSurface CreateDrawingSurface(WriteableBitmap bitmap)
    {
        return DrawingSurface.Create(
            new ImageInfo(bitmap.PixelWidth, bitmap.PixelHeight, ColorType.Bgra8888, AlphaType.Premul, ColorSpace.CreateSrgb()),
            bitmap.BackBuffer,
            bitmap.BackBufferStride);
    }

    public StructureMemberViewModel(DocumentViewModel doc, DocumentInternalParts internals, Guid guidValue)
    {
        Document = doc;
        Internals = internals;

        this.guidValue = guidValue;
        VecI previewSize = CalculatePreviewSize(doc.SizeBindable);
        PreviewBitmap = null;
        PreviewSurface = null;
    }
}
