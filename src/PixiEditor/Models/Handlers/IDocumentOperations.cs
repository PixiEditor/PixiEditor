namespace PixiEditor.Models.Handlers;

public interface IDocumentOperations
{
    public void DeleteStructureMember(Guid memberGuidValue);
    public void DuplicateLayer(Guid memberGuidValue);
    public void AddSoftSelectedMember(Guid memberGuidValue);
}
