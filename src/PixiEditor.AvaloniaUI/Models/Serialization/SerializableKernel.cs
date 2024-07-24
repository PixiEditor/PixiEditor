using MessagePack;

namespace PixiEditor.AvaloniaUI.Models.Serialization;

[MessagePackObject]
public class SerializableKernel
{
    [Key(0)]
    public int Width { get; set; }
    
    [Key(1)]
    public int Height { get; set; }
    
    [Key(2)]
    public float[] Values { get; set; }
}
