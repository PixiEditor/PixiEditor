using System;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditorPrototype.Models;
using SkiaSharp;

namespace PixiEditorPrototype.ViewModels;

internal abstract class StructureMemberViewModel : INotifyPropertyChanged
{
    protected IReadOnlyStructureMember member;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected DocumentViewModel Document { get; }
    protected DocumentHelpers Helpers { get; }

    public string Name
    {
        get => member.Name;
        set => Helpers.ActionAccumulator.AddFinishedActions(new StructureMemberName_Action(member.GuidValue, value));
    }

    public bool IsVisible
    {
        get => member.IsVisible;
        set => Helpers.ActionAccumulator.AddFinishedActions(new StructureMemberIsVisible_Action(value, member.GuidValue));
    }

    public BlendMode BlendMode
    {
        get => member.BlendMode;
        set => Helpers.ActionAccumulator.AddFinishedActions(new StructureMemberBlendMode_Action(value, member.GuidValue));
    }

    public bool ClipToMemberBelowEnabled
    {
        get => member.ClipToMemberBelow;
        set => Helpers.ActionAccumulator.AddFinishedActions(new StructureMemberClipToMemberBelow_Action(value, member.GuidValue));
    }

    public bool IsSelected { get; set; }
    public bool ShouldDrawOnMask { get; set; }

    public float Opacity => member.Opacity;

    public Guid GuidValue => member.GuidValue;

    public bool HasMask => member.ReadOnlyMask is not null;

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

    public StructureMemberViewModel(DocumentViewModel doc, DocumentHelpers helpers, IReadOnlyStructureMember member)
    {
        this.member = member;
        Document = doc;
        Helpers = helpers;
        MoveUpCommand = new(_ => Helpers.StructureHelper.MoveStructureMember(GuidValue, false));
        MoveDownCommand = new(_ => Helpers.StructureHelper.MoveStructureMember(GuidValue, true));
        UpdateOpacityCommand = new(UpdateOpacity);
        EndOpacityUpdateCommand = new(EndOpacityUpdate);

        var previewSize = CalculatePreviewSize(new(doc.Width, doc.Height));
        PreviewBitmap = new WriteableBitmap(previewSize.X, previewSize.Y, 96, 96, PixelFormats.Pbgra32, null);
        PreviewSurface = SKSurface.Create(new SKImageInfo(previewSize.X, previewSize.Y, SKColorType.Bgra8888), PreviewBitmap.BackBuffer, PreviewBitmap.BackBufferStride);
        if (member.ReadOnlyMask is not null)
        {
            MaskPreviewBitmap = new WriteableBitmap(previewSize.X, previewSize.Y, 96, 96, PixelFormats.Pbgra32, null);
            MaskPreviewSurface = SKSurface.Create(new SKImageInfo(previewSize.X, previewSize.Y, SKColorType.Bgra8888), PreviewBitmap.BackBuffer, PreviewBitmap.BackBufferStride);
        }
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
