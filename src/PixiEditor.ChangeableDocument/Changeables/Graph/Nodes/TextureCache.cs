using Drawie.Backend.Core;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class TextureCache
{
    private Dictionary<ChunkResolution, Texture> _cachedTextures = new();
    
    public Texture GetTexture(ChunkResolution resolution, VecI size)
    {
        if (_cachedTextures.TryGetValue(resolution, out var texture) && texture.Size == size)
        {
            return texture;
        }

        texture = new Texture(size);
        _cachedTextures[resolution] = texture;
        return texture;
    }

    public void Dispose()
    {
        foreach (var texture in _cachedTextures.Values)
        {
            texture.Dispose();
        }
    }
}
