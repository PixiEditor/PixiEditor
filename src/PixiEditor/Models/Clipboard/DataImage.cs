using ChunkyImageLib;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace PixiEditor.Models.Clipboard;

public record struct DataImage
{
    public string? Name { get; set; }
    public Surface Image { get; set; }
    public VecI Position { get; set; }
    
    public DataImage(Surface image, VecI position) : this(null, image, position) { }

    public DataImage(string? name, Surface image, VecI position)
    {
        Name = name;
        Image = image;
        Position = position;
    }
}
