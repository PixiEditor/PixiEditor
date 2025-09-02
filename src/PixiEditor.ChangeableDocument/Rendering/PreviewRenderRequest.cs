using Drawie.Backend.Core;

namespace PixiEditor.ChangeableDocument.Rendering;

public record struct PreviewRenderRequest
{
    public Texture? Texture { get; set; }
    public string? ElementToRender { get; set; }

    public PreviewRenderRequest(Texture? texture, string? elementToRender = null)
    {
        Texture = texture;
        ElementToRender = elementToRender;
    }
}
