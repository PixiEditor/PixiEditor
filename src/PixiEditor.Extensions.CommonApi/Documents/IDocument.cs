using PixiEditor.Extensions.CommonApi.Palettes;

namespace PixiEditor.Extensions.CommonApi.Documents;

public interface IDocument
{
    public Guid Id { get; }
    public void Resize(int width, int height);
    PaletteColor[] Palette { get; set; }
}
