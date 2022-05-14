using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.ChangeInfos;

namespace PixiEditorPrototype.Models;

internal record class RefreshViewport_PassthroughAction(ViewportLocation Location) : IAction, IChangeInfo;
