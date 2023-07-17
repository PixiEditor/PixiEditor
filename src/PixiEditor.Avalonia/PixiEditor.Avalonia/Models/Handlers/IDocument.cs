using System.Collections.Generic;
using PixiEditor.Extensions.Palettes;

namespace PixiEditor.Models.Containers;

public interface IDocument
{
    public List<PaletteColor> Palette { get; set; }
}
