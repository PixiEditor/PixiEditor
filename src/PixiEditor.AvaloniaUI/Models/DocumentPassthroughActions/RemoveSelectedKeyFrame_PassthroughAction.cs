﻿using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.ChangeInfos;

namespace PixiEditor.AvaloniaUI.Models.DocumentPassthroughActions;

internal record RemoveSelectedKeyFrame_PassthroughAction(Guid KeyFrameGuid) : IChangeInfo, IAction;