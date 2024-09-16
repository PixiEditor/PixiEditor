using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.ChangeInfos;

namespace PixiEditor.Models.DocumentModels.Public;

internal class InvokeAction_PassthroughAction(Action action) : IAction, IChangeInfo
{
    public Action Action { get; } = action;
}
