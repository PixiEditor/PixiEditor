using Drawie.Backend.Core;

namespace PixiEditor.ChangeableDocument.Rendering;

public record struct PreviewRenderRequest
{
    public Texture? Texture { get; set; }
    public string? ElementToRender { get; set; }
    public Action TextureUpdatedAction { get; set; }

    public PreviewRenderRequest(Texture? texture, Action textureUpdatedAction, string? elementToRender = null)
    {
        Texture = texture;
        TextureUpdatedAction = textureUpdatedAction;
        ElementToRender = elementToRender;
    }

    public void InvokeTextureUpdated()
    {
        TextureUpdatedAction?.Invoke();
    }
}
