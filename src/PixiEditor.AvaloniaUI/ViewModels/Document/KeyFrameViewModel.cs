using ChunkyImageLib;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.AvaloniaUI.Models.DocumentModels;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Surfaces;

namespace PixiEditor.AvaloniaUI.ViewModels.Document;

internal abstract class KeyFrameViewModel : ObservableObject, IKeyFrameHandler
{
    private Surface? previewSurface;
    private int startFrameBindable;
    private int durationBindable;
    private bool isVisibleBindable = true;
    private bool isSelected;

    public DocumentViewModel Document { get; }
    protected DocumentInternalParts Internals { get; }

    IDocument IKeyFrameHandler.Document => Document;

    public Surface? PreviewSurface
    {
        get => previewSurface;
        set => SetProperty(ref previewSurface, value);
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

            if (!Document.UpdateableChangeActive)
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

            if (!Document.UpdateableChangeActive)
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
            if(!Document.UpdateableChangeActive)
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

    protected KeyFrameViewModel(int startFrame, int duration, Guid layerGuid, Guid id,
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
}
