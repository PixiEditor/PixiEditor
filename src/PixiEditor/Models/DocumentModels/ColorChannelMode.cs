using System.Diagnostics.Contracts;

namespace PixiEditor.Models.DocumentModels;

internal record struct ColorChannelMode(bool IsVisible, bool IsSolo)
{
    [Pure]
    public ColorChannelMode WithVisible(bool visible) => this with { IsVisible = visible };
    
    [Pure]
    public ColorChannelMode WithSolo(bool solo) => this with { IsSolo = solo };

    public static ColorChannelMode Default => new(true, false);
}
