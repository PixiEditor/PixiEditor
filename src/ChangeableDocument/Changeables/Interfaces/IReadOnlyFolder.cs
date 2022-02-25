namespace ChangeableDocument.Changeables.Interfaces
{
    public interface IReadOnlyFolder : IReadOnlyStructureMember
    {
        IReadOnlyList<IReadOnlyStructureMember> ReadOnlyChildren { get; }
    }
}
