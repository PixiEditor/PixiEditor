﻿using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.ChangeInfos;

namespace PixiEditor.Models.DocumentPassthroughActions;
internal record class RemoveViewport_PassthroughAction(Guid GuidValue) : IAction, IChangeInfo;
