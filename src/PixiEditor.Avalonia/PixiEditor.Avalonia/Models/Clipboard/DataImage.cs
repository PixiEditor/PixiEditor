using ChunkyImageLib;
using PixiEditor.DrawingApi.Core.Numerics;

public record struct DataImage(string? name, Surface image, VecI position)
{
    public DataImage(Surface image, VecI position) : this(null, image, position) { }
}
