using ChunkyImageLib;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.AvaloniaUI.Models.Handlers;

namespace PixiEditor.AvaloniaUI.ViewModels.Document;

public abstract class KeyFrameViewModel(int startFrame, int duration, Guid layerGuid, Guid id) : ObservableObject, IKeyFrameHandler
{
    private Surface? previewSurface;
    private int _startFrame = startFrame;
    private int _duration = duration;
    
    public Surface? PreviewSurface
    {
        get => previewSurface;
        set => SetProperty(ref previewSurface, value);
    }

    public virtual int StartFrame
    {
        get => _startFrame;
        set
        {
            if (value < 0)
            {
                value = 0;
            }
            
            SetProperty(ref _startFrame, value);
        }
    }
    public virtual int Duration
    {
        get => _duration;
        set
        {
            if (value < 1)
            {
                value = 1;
            }
            
            SetProperty(ref _duration, value);
        }
    }

    public Guid LayerGuid { get; } = layerGuid;
    public Guid Id { get; } = id;
}
