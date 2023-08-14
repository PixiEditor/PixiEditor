namespace PixiEditor.AvaloniaUI.Models.Handlers;

public interface IDocumentOperations
{
    public void DeleteStructureMember(Guid memberGuidValue);
    public void DuplicateLayer(Guid memberGuidValue);
}
