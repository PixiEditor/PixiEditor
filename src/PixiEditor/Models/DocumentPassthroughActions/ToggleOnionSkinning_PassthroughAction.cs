using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.ChangeInfos;

namespace PixiEditor.Models.DocumentPassthroughActions;

internal record ToggleOnionSkinning_PassthroughAction(bool IsOnionSkinningEnabled) : IAction, IChangeInfo;
