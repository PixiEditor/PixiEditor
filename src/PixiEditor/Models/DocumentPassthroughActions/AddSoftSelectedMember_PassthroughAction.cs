using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.ChangeInfos;

namespace PixiEditor.Models.DocumentPassthroughActions;
internal record class AddSoftSelectedMember_PassthroughAction(Guid GuidValue) : IChangeInfo, IAction;
