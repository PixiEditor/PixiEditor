using ChangeableDocument.Changeables.Interfaces;

namespace ChangeableDocument.Changeables
{
    internal class Folder : StructureMember, IReadOnlyFolder
    {
        internal List<StructureMember> Children { get; set; } = new();
        public IReadOnlyList<IReadOnlyStructureMember> ReadOnlyChildren => Children;

        internal override Folder Clone()
        {
            List<StructureMember> clonedChildren = new();
            foreach (var child in Children)
            {
                clonedChildren.Add(child.Clone());
            }

            return new Folder()
            {
                GuidValue = GuidValue,
                IsVisible = IsVisible,
                Name = Name,
                Opacity = Opacity,
                Children = clonedChildren
            };
        }
    }
}
