using System;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.ChangeInfos;

namespace PixiEditorPrototype.Models
{
    internal record class RefreshViewport_PassthroughAction(Guid GuidValue) : IAction, IChangeInfo;
}
