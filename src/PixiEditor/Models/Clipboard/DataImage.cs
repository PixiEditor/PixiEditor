using ChunkyImageLib;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.Models.Clipboard;

public record struct DataImage(string? name, Surface image, VecI position)
{
    public DataImage(Surface image, VecI position) : this(null, image, position) { }
}
