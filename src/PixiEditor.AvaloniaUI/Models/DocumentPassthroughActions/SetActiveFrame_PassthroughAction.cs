using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.ChangeInfos;

namespace PixiEditor.AvaloniaUI.Models.DocumentPassthroughActions;

internal record SetActiveFrame_PassthroughAction(int Frame) : IChangeInfo, IAction;
