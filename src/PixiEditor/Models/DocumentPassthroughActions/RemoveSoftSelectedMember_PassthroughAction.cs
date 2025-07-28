using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.ChangeInfos;

namespace PixiEditor.Models.DocumentPassthroughActions;
internal record class RemoveSoftSelectedMember_PassthroughAction(Guid Id) : IAction, IChangeInfo;
