using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class TextureCache : IDisposable
{
    private Dictionary<int, Texture> _managedTextures = new();

    public Texture RequestTexture(int id, VecI size, ColorSpace processingCs, bool clear = true)
    {
        if (_managedTextures.TryGetValue(id, out var texture))
        {
            if (texture.Size != size || texture.IsDisposed || texture.ColorSpace != processingCs)
            {
                texture.Dispose();
                texture = new Texture(CreateImageInfo(size, processingCs));
                _managedTextures[id] = texture;
                return texture;
            }

            if (clear)
            {
                texture.DrawingSurface.Canvas.Clear(Colors.Transparent);
                texture.DrawingSurface.Canvas.SetMatrix(Matrix3X3.Identity);
            }

            return texture;
        }

        _managedTextures[id] = new Texture(CreateImageInfo(size, processingCs));
        return _managedTextures[id];
    }

    private ImageInfo CreateImageInfo(VecI size, ColorSpace processingCs)
    {
        if (processingCs == null)
        {
            return new ImageInfo(size.X, size.Y, ColorType.RgbaF16, AlphaType.Premul, ColorSpace.CreateSrgbLinear())
            {
                GpuBacked = true
            };
        }

        return new ImageInfo(size.X, size.Y, ColorType.RgbaF16, AlphaType.Premul, processingCs) { GpuBacked = true };
    }

    public void Dispose()
    {
        foreach (var texture in _managedTextures)
        {
            texture.Value.Dispose();
        }
    }
}
