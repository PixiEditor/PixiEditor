using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.ChangeInfos;

namespace PixiEditor.Models.DocumentPassthroughActions;

public record SetPlayingState_PassthroughAction(bool Play) : IAction, IChangeInfo;
