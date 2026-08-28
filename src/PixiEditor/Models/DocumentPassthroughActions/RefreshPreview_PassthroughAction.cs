using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.ChangeInfos;

namespace PixiEditor.Models.DocumentPassthroughActions;

internal record class RefreshPreview_PassthroughAction(Guid Id, Guid? SubId = null, string ElementToRender = null) : IAction, IChangeInfo;
