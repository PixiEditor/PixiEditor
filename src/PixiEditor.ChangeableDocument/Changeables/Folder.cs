using System.Collections.Immutable;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables;

internal class Folder : StructureMember, IReadOnlyFolder
{
    // Don't forget to update CreateFolder_ChangeInfo, DocumentUpdater.ProcessCreateStructureMember, and Folder.Clone when adding new properties
    /// <summary>
    /// The children of the folder
    /// </summary>
    public ImmutableList<StructureMember> Children { get; set; } = ImmutableList<StructureMember>.Empty;
    IReadOnlyList<IReadOnlyStructureMember> IReadOnlyFolder.Children => Children;

    /// <summary>
    /// Creates a clone of the folder, its mask and all of its children
    /// </summary>
    internal override Folder Clone()
    {
        var builder = ImmutableList<StructureMember>.Empty.ToBuilder();
        for (var i = 0; i < Children.Count; i++)
        {
            var child = Children[i];
            builder.Add(child.Clone());
        }

        return new Folder
        {
            GuidValue = GuidValue,
            IsVisible = IsVisible,
            Name = Name,
            Opacity = Opacity,
            Children = builder.ToImmutable(),
            Mask = Mask?.CloneFromCommitted(),
            BlendMode = BlendMode,
            ClipToMemberBelow = ClipToMemberBelow,
            MaskIsVisible = MaskIsVisible
        };
    }

    /// <summary>
    /// Disposes all children and the mask
    /// </summary>
    public override void Dispose()
    {
        foreach (var child in Children)
        {
            child.Dispose();
        }
        Mask?.Dispose();
    }
}
