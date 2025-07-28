using PixiEditor.ChangeableDocument;
using PixiEditor.Models.DocumentModels.Public;
using PixiEditor.Models.Layers;

namespace PixiEditor.Models.Handlers;

internal interface IDocumentOperations
{
    public void DeleteStructureMember(Guid memberGuidValue);
    public Guid? DuplicateMember(Guid memberGuidValue);
    public void AddSoftSelectedMember(Guid memberGuidValue);
    public void MoveStructureMember(Guid memberGuidValue, Guid target, StructureMemberPlacement placement);
    public void SetSelectedMember(Guid memberId);
    public void ClearSoftSelectedMembers();
    public Guid? CreateStructureMember(Type type, ActionSource source, string? name = null);
    public void InvokeCustomAction(Action action, bool stopActiveExecutor = true);
    public ChangeBlock StartChangeBlock();
}
