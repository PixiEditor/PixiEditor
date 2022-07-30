using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Enums;

namespace PixiEditor.ViewModels.SubViewModels.Document;
#nullable enable
internal abstract class StructureMemberViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public DocumentViewModel Document { get; }
    protected DocumentHelpers Helpers { get; }


    private string name = "";
    public void SetName(string name)
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
                Helpers.ActionAccumulator.AddFinishedActions(new StructureMemberName_Action(GuidValue, value));
        }
    }

    private bool isVisible;
    public void SetIsVisible(bool isVisible)
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
                Helpers.ActionAccumulator.AddFinishedActions(new StructureMemberIsVisible_Action(value, GuidValue));
        }
    }

    private bool maskIsVisible;
    public void SetMaskIsVisible(bool maskIsVisible)
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
                Helpers.ActionAccumulator.AddFinishedActions(new StructureMemberMaskIsVisible_Action(value, GuidValue));
        }
    }

    private BlendMode blendMode;
    public void SetBlendMode(BlendMode blendMode)
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
                Helpers.ActionAccumulator.AddFinishedActions(new StructureMemberBlendMode_Action(value, GuidValue));
        }
    }

    private bool clipToMemberBelowEnabled;
    public void SetClipToMemberBelowEnabled(bool clipToMemberBelowEnabled)
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
                Helpers.ActionAccumulator.AddFinishedActions(new StructureMemberClipToMemberBelow_Action(value, GuidValue));
        }
    }

    private bool hasMask;
    public void SetHasMask(bool hasMask)
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

    public void SetOpacity(float opacity)
    {
        this.opacity = opacity;
        RaisePropertyChanged(nameof(OpacityBindable));
    }
    public float OpacityBindable
    {
        get => opacity;
        // this is stupid. This setter shouldn't actually exist, but it has to because the NumberInput control is badly designed.
        // You can't bind it's value using a OneWay binding, because it gets overwritten by NumberInput internally when it assigns something to Value
        // This doesn't happen with TwoWay bindings because they behave differently and forward the value you set instead of getting replaced
        // So really I just need a OneWay binding, but I'm forced to use a TwoWay binding with a setter
        // The binding is in LayersManager's opacity field btw.
        set { }
    }

    public StructureMemberSelectionType Selection { get; set; }

    public const int PreviewSize = 48;
    public WriteableBitmap PreviewBitmap { get; set; }
    public SKSurface PreviewSurface { get; set; }

    public WriteableBitmap? MaskPreviewBitmap { get; set; }
    public SKSurface? MaskPreviewSurface { get; set; }

    public void RaisePropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public static VecI CalculatePreviewSize(VecI docSize)
    {
        double proportions = docSize.Y / (double)docSize.X;
        const int prSize = StructureMemberViewModel.PreviewSize;
        return proportions > 1 ?
            new VecI((int)Math.Round(prSize / proportions), prSize) :
            new VecI(prSize, (int)Math.Round(prSize * proportions));
    }

    public StructureMemberViewModel(DocumentViewModel doc, DocumentHelpers helpers, Guid guidValue)
    {
        Document = doc;
        Helpers = helpers;

        this.guidValue = guidValue;
        VecI previewSize = CalculatePreviewSize(doc.SizeBindable);
        PreviewBitmap = new WriteableBitmap(previewSize.X, previewSize.Y, 96, 96, PixelFormats.Pbgra32, null);
        PreviewSurface = SKSurface.Create(new SKImageInfo(previewSize.X, previewSize.Y, SKColorType.Bgra8888), PreviewBitmap.BackBuffer, PreviewBitmap.BackBufferStride);
    }
}
