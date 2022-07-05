using System;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditorPrototype.Models;
using SkiaSharp;

namespace PixiEditorPrototype.ViewModels;

internal abstract class StructureMemberViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected DocumentViewModel Document { get; }
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
        set => Helpers.ActionAccumulator.AddFinishedActions(new StructureMemberName_Action(GuidValue, value));
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
        set => Helpers.ActionAccumulator.AddFinishedActions(new StructureMemberIsVisible_Action(value, GuidValue));
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
        set => Helpers.ActionAccumulator.AddFinishedActions(new StructureMemberMaskIsVisible_Action(value, GuidValue));
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
        set => Helpers.ActionAccumulator.AddFinishedActions(new StructureMemberBlendMode_Action(value, GuidValue));
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
        set => Helpers.ActionAccumulator.AddFinishedActions(new StructureMemberClipToMemberBelow_Action(value, GuidValue));
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
    }

    private bool isSelected;
    public bool IsSelected
    {
        get => isSelected;
        set
        {
            isSelected = value;
            Document.RaisePropertyChanged(nameof(Document.SelectedStructureMember));
        }
    }
    public bool ShouldDrawOnMask { get; set; }


    public const int PreviewSize = 48;
    public WriteableBitmap PreviewBitmap { get; set; }
    public SKSurface PreviewSurface { get; set; }

    public WriteableBitmap? MaskPreviewBitmap { get; set; }
    public SKSurface? MaskPreviewSurface { get; set; }

    public RelayCommand MoveUpCommand { get; }
    public RelayCommand MoveDownCommand { get; }
    public RelayCommand UpdateOpacityCommand { get; }
    public RelayCommand EndOpacityUpdateCommand { get; }

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

        MoveUpCommand = new(_ => Helpers.StructureHelper.MoveStructureMember(GuidValue, false));
        MoveDownCommand = new(_ => Helpers.StructureHelper.MoveStructureMember(GuidValue, true));
        UpdateOpacityCommand = new(UpdateOpacity);
        EndOpacityUpdateCommand = new(EndOpacityUpdate);

        this.guidValue = guidValue;
        var previewSize = CalculatePreviewSize(doc.SizeBindable);
        PreviewBitmap = new WriteableBitmap(previewSize.X, previewSize.Y, 96, 96, PixelFormats.Pbgra32, null);
        PreviewSurface = SKSurface.Create(new SKImageInfo(previewSize.X, previewSize.Y, SKColorType.Bgra8888), PreviewBitmap.BackBuffer, PreviewBitmap.BackBufferStride);
    }

    private void EndOpacityUpdate(object? opacity)
    {
        Helpers.ActionAccumulator.AddFinishedActions(new EndStructureMemberOpacity_Action());
    }

    private void UpdateOpacity(object? opacity)
    {
        if (opacity is not double value)
            throw new ArgumentException("The passed value isn't a double");
        Helpers.ActionAccumulator.AddActions(new StructureMemberOpacity_Action(GuidValue, (float)value));
    }
}
