using ProtoBuf;

namespace PixiEditor.Extensions.CommonApi.Palettes;

[ProtoContract]
public class ExtensionPalette : IPalette
{
    [ProtoMember(1)]
    public string Name { get; }
    
    [ProtoMember(2)]
    public List<PaletteColor> Colors { get; }
    
    [ProtoMember(3)]
    public bool IsFavourite { get; set; }
    
    [ProtoMember(4)]
    public string FileName { get; set; }
    
    [ProtoIgnore]
    public PaletteListDataSource Source { get; set; }

    public ExtensionPalette(string name, List<PaletteColor> colors, PaletteListDataSource source)
    {
        Name = name;
        Colors = colors;
        Source = source;
    }
    
    public ExtensionPalette()
    {
    }
}
