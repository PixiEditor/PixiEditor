using Avalonia;
using Avalonia.Media;
using ChunkyImageLib;
using PixiEditor.DrawingApi.Core;

namespace PixiEditor.Views.Visuals;

public class TextureImage : IImage
{
    public Texture Texture { get; set; }
    public Stretch Stretch { get; set; } = Stretch.Uniform;

    public Size Size { get; }

    public TextureImage(Texture texture)
    {
        Texture = texture;
        Size = new Size(texture.Size.X, texture.Size.Y);
    }

    public void Draw(DrawingContext context, Rect sourceRect, Rect destRect)
    {
        context.Custom(new DrawTextureOperation(destRect, Stretch, Texture));
    }
}
