namespace PixiEditor.Models.Handlers;

public interface IDocumentReferenceData
{
    public Dictionary<Guid, HashSet<Guid>> ReferencingNodes { get; }
    public string? OriginalFilePath { get; }
    public Guid ReferenceId { get; }

    public void AddReferencingNode(Guid documentId, Guid nodeId);
    public void RemoveReferencingNode(Guid documentId, Guid nodeId);
}
