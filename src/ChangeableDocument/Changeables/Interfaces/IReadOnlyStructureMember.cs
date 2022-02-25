namespace ChangeableDocument.Changeables.Interfaces
{
    public interface IReadOnlyStructureMember
    {
        bool IsVisible { get; }
        string Name { get; }
        Guid GuidValue { get; }
    }
}
