using ChunkyImageLib;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.AvaloniaUI.Models.Handlers;

namespace PixiEditor.AvaloniaUI.ViewModels.Document;

public abstract class KeyFrameViewModel(int startFrame, int duration, Guid layerGuid, Guid id) : ObservableObject, IKeyFrameHandler
{
    private Surface? previewSurface;
    
    public Surface? PreviewSurface
    {
        get => previewSurface;
        set => SetProperty(ref previewSurface, value);
    }
    
    public virtual int StartFrame { get; } = startFrame;
    public virtual int Duration { get; } = duration;
    public Guid LayerGuid { get; } = layerGuid;
    public Guid Id { get; } = id;
}
