using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.ChangeInfos;

namespace PixiEditor.Models.DocumentPassthroughActions;

internal record class RefreshPreviews_PassthroughAction() : IAction, IChangeInfo;
