using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.ChangeInfos;

namespace PixiEditor.Models.DocumentPassthroughActions;

public record SetOnionFrames_PassthroughAction(int Frames) : IChangeInfo, IAction;
