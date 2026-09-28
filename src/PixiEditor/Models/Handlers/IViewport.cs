namespace PixiEditor.Models.Handlers;

public interface IViewport
{
    public string? RenderOutputName { get; set; }
    public Guid SceneTextureKey { get; set; }
}
