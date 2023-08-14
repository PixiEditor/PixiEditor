using PixiEditor.AvaloniaUI.Models.Position;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.ChangeInfos;

namespace PixiEditor.AvaloniaUI.Models.DocumentPassthroughActions;

internal record class RefreshViewport_PassthroughAction(ViewportInfo Info) : IAction, IChangeInfo;
