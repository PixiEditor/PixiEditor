using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables;

public class DocumentReference : ICloneable
{
    public string? OriginalFilePath { get; set; }
    public Guid ReferenceId { get; set; }
    public IReadOnlyDocument DocumentInstance { get; set; }

    public DocumentReference(string? originalFilePath, Guid referenceId, IReadOnlyDocument documentInstance)
    {
        OriginalFilePath = originalFilePath;
        ReferenceId = referenceId;
        DocumentInstance = documentInstance;
    }

    public object Clone()
    {
        return new DocumentReference(OriginalFilePath, ReferenceId, DocumentInstance.Clone());
    }
}
