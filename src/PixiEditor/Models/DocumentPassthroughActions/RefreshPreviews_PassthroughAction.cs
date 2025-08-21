using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.Models.Position;

namespace PixiEditor.Models.DocumentPassthroughActions;

internal record class RefreshPreviews_PassthroughAction() : IAction, IChangeInfo;
