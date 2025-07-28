using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.ChangeInfos;

namespace PixiEditor.Models.DocumentPassthroughActions;

internal record ClearSelectedKeyFrames_PassthroughAction() : IChangeInfo, IAction;
