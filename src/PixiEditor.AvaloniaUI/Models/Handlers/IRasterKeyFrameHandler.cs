namespace PixiEditor.AvaloniaUI.Models.Handlers;

public interface IRasterKeyFrameHandler : IKeyFrameHandler
{
    public Guid TargetLayerGuid { get; }
}
