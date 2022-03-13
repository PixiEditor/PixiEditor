using ChangeableDocument.Changeables.Interfaces;

namespace ChangeableDocument.Changeables
{
    internal abstract class StructureMember : IChangeable, IReadOnlyStructureMember
    {
        public float ReadOnlyOpacity { get; set; } = 1f;
        public bool ReadOnlyIsVisible { get; set; } = true;
        public string ReadOnlyName { get; set; } = "Unnamed";
        public Guid ReadOnlyGuidValue { get; init; }

        internal abstract StructureMember Clone();
    }
}
