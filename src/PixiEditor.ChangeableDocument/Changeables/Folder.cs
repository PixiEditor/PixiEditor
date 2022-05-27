using System.Collections.Immutable;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables;

internal class Folder : StructureMember, IReadOnlyFolder
{
    public ImmutableList<StructureMember> Children { get; set; } = ImmutableList<StructureMember>.Empty;
    IReadOnlyList<IReadOnlyStructureMember> IReadOnlyFolder.Children => Children;

    internal override Folder Clone()
    {
        var builder = ImmutableList<StructureMember>.Empty.ToBuilder();
        for (var i = 0; i < Children.Count; i++)
        {
            var child = Children[i];
            builder.Add(child.Clone());
        }

        return new Folder()
        {
            GuidValue = GuidValue,
            IsVisible = IsVisible,
            Name = Name,
            Opacity = Opacity,
            Children = builder.ToImmutable(),
            Mask = Mask?.CloneFromCommitted()
        };
    }

    public override void Dispose()
    {
        for (var i = 0; i < Children.Count; i++)
        {
            Children[i].Dispose();
        }
        Mask?.Dispose();
    }
}
