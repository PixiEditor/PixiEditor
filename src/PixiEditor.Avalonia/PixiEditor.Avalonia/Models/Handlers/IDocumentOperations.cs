namespace PixiEditor.Models.Containers;

public interface IDocumentOperations
{
    public void DeleteStructureMember(Guid memberGuidValue);
    public void DuplicateLayer(Guid memberGuidValue);
}
