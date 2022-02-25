using ChangeableDocument.Changeables.Interfaces;

namespace ChangeableDocument.Changeables
{
    internal abstract class StructureMember : IChangeable, IReadOnlyStructureMember
    {
        public bool IsVisible { get; set; } = true;
        public string Name { get; set; } = "Unnamed";
        public Guid GuidValue { get; init; }

        internal abstract StructureMember Clone();
    }
}
