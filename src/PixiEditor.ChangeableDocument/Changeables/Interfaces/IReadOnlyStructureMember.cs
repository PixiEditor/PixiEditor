using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IReadOnlyStructureMember
{
    /// <summary>
    /// Is the member visible. Defaults to true
    /// </summary>
    bool IsVisible { get; }

    /// <summary>
    /// The opacity of the member (Ranging from 0f to 1f)
    /// </summary>
    float Opacity { get; }
    bool ClipToMemberBelow { get; }

    /// <summary>
    /// The name of the member. Defaults to "Unnamed"
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The guid of the member
    /// </summary>
    Guid GuidValue { get; }

    /// <summary>
    /// The blend mode of the member
    /// </summary>
    BlendMode BlendMode { get; }

    /// <summary>
    /// Is the mask of the member visible. Defaults to true
    /// </summary>
    bool MaskIsVisible { get; }

    /// <summary>
    /// The mask of the member
    /// </summary>
    IReadOnlyChunkyImage? Mask { get; }

    /// <summary>
    ///     The tight bounds of the member. The bounds are the smallest rectangle that contains all the pixels of the member.
    /// </summary>
    /// <returns>The tight bounds of the member</returns>
    public RectI? GetTightBounds(int frame);
}
