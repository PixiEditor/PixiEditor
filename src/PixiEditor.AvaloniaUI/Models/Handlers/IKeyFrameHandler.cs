namespace PixiEditor.AvaloniaUI.Models.Handlers;

public interface IKeyFrameHandler
{
    public int StartFrame { get; }
    public int Duration { get; }
    public Guid LayerGuid { get; }
    public Guid Id { get; }
}
