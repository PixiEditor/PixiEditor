using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.Models.Position;

namespace PixiEditor.Models.DocumentPassthroughActions;

internal record class RefreshPreview_PassthroughAction(Guid Id) : IAction, IChangeInfo;
