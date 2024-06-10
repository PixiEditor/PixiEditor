namespace PixiEditor.AvaloniaUI.Models.Handlers;

public interface IRasterClipHandler : IClipHandler
{
    public Guid TargetLayerGuid { get; }
}
