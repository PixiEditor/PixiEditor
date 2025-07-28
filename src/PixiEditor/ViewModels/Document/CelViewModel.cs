using ChunkyImageLib;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.ChangeableDocument.Actions.Generated;
using Drawie.Backend.Core;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Rendering;

namespace PixiEditor.ViewModels.Document;

internal abstract class CelViewModel : ObservableObject, ICelHandler
{
    private PreviewPainter? previewPainter;
    private int startFrameBindable;
    private int durationBindable;
    private bool isVisibleBindable = true;
    private bool isSelected;
    private bool isCollapsed;

    public bool IsCollapsed
    {
        get => isCollapsed;
        set => SetProperty(ref isCollapsed, value);
    }

    public DocumentViewModel Document { get; }

    protected DocumentInternalParts Internals { get; }

    IDocument ICelHandler.Document => Document;

    public PreviewPainter? PreviewPainter
    {
        get => previewPainter;
        set => SetProperty(ref previewPainter, value);
    }

    public virtual int StartFrameBindable
    {
        get => startFrameBindable;
        set
        {
            if (value < 0)
            {
                value = 0;
            }

            if (!Document.BlockingUpdateableChangeActive)
            {
                Internals.ActionAccumulator.AddFinishedActions(
                    new KeyFrameLength_Action(Id, value, DurationBindable),
                    new EndKeyFrameLength_Action());
            }
        }
    }

    public virtual int DurationBindable
    {
        get => durationBindable;
        set
        {
            if (value < 1)
            {
                value = 1;
            }

            if (!Document.BlockingUpdateableChangeActive)
            {
                Internals.ActionAccumulator.AddFinishedActions(
                    new KeyFrameLength_Action(Id, StartFrameBindable, value),
                    new EndKeyFrameLength_Action());
            }
        }
    }

    public virtual bool IsVisible
    {
        get => isVisibleBindable;
        set
        {
            if (!Document.BlockingUpdateableChangeActive)
            {
                Internals.ActionAccumulator.AddFinishedActions(new KeyFrameVisibility_Action(Id, value));
            }
        }
    }

    public Guid LayerGuid { get; }
    public Guid Id { get; }

    public bool IsSelected
    {
        get => isSelected;
        set => SetProperty(ref isSelected, value);
    }

    protected CelViewModel(int startFrame, int duration, Guid layerGuid, Guid id,
        DocumentViewModel document, DocumentInternalParts internalParts)
    {
        startFrameBindable = startFrame;
        durationBindable = duration;
        LayerGuid = layerGuid;
        Id = id;
        Document = document;
        Internals = internalParts;
    }

    public void SetStartFrame(int newStartFrame)
    {
        startFrameBindable = newStartFrame;
        OnPropertyChanged(nameof(StartFrameBindable));
    }

    public void SetDuration(int newDuration)
    {
        durationBindable = newDuration;
        OnPropertyChanged(nameof(DurationBindable));
    }

    public void ChangeFrameLength(int newStartFrame, int newDuration)
    {
        newStartFrame = Math.Max(0, newStartFrame);
        newDuration = Math.Max(1, newDuration);
        Internals.ActionAccumulator.AddActions(
            new KeyFrameLength_Action(Id, newStartFrame, newDuration));
    }

    public void EndChangeFrameLength()
    {
        Internals.ActionAccumulator.AddFinishedActions(new EndKeyFrameLength_Action());
    }

    public virtual void SetVisibility(bool isVisible)
    {
        isVisibleBindable = isVisible;
        OnPropertyChanged(nameof(IsVisible));
    }

    public bool IsWithinRange(int frame)
    {
        return frame >= StartFrameBindable && frame < StartFrameBindable + DurationBindable;
    }

    public void Dispose()
    {
        PreviewPainter?.Dispose();
        PreviewPainter = null;
    }
}
