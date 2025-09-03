using Drawie.Backend.Core;

namespace PixiEditor.ChangeableDocument.Rendering;

public record struct PreviewRenderRequest
{
    public Texture? Texture
    {
        get
        {
            if (!accessedTexture)
            {
                texture = textureCreateFunc();
                accessedTexture = true;
            }

            return texture;
        }
    }
    public string? ElementToRender { get; set; }
    public Action TextureUpdatedAction { get; set; }

    private Func<Texture?> textureCreateFunc;
    private Texture? texture;
    private bool accessedTexture = false;

    public PreviewRenderRequest(Func<Texture?> textureCreateFunc, Action textureUpdatedAction, string? elementToRender = null)
    {
        this.textureCreateFunc = textureCreateFunc;
        TextureUpdatedAction = textureUpdatedAction;
        ElementToRender = elementToRender;
    }

    public void InvokeTextureUpdated()
    {
        TextureUpdatedAction?.Invoke();
    }
}
