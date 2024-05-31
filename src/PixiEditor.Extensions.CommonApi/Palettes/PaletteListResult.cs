using System.Runtime.Serialization;
using ProtoBuf;

namespace PixiEditor.Extensions.CommonApi.Palettes;

[ProtoContract]
public class PaletteListResult
{
    [ProtoMember(1)]
    public ExtensionPalette[] Palettes { get; set; }

    public PaletteListResult(ExtensionPalette[] palettes)
    {
        Palettes = palettes;
    }
    
    public PaletteListResult()
    {
    }
}
