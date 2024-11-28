using PixiEditor.ChangeableDocument;
using PixiEditor.Models.Layers;

namespace PixiEditor.Models.Handlers;

internal interface IDocumentOperations
{
    public void DeleteStructureMember(Guid memberGuidValue);
    public void DuplicateLayer(Guid memberGuidValue);
    public void AddSoftSelectedMember(Guid memberGuidValue);
    public void MoveStructureMember(Guid memberGuidValue, Guid target, StructureMemberPlacement placement);
    public void SetSelectedMember(Guid memberId);
    public void ClearSoftSelectedMembers();
    public Guid? CreateStructureMember(Type type, ActionSource source, string? name = null);
}
