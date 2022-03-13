namespace ChangeableDocument.Changeables.Interfaces
{
    public interface IReadOnlyStructureMember
    {
        bool ReadOnlyIsVisible { get; }
        string ReadOnlyName { get; }
        Guid ReadOnlyGuidValue { get; }
        float ReadOnlyOpacity { get; }
    }
}
