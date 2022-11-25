using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.ChangeInfos;

namespace PixiEditor.Models.DocumentPassthroughActions;
internal record class RemoveSoftSelectedMember_PassthroughAction(Guid GuidValue) : IAction, IChangeInfo;
