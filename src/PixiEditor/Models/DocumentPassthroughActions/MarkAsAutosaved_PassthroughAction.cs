using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.ChangeInfos;

namespace PixiEditor.Models.DocumentPassthroughActions;

internal enum DocumentMarkType
{
    Saved,
    Unsaved,
    Autosaved,
    UnAutosaved
}

internal record class MarkAsAutosaved_PassthroughAction(DocumentMarkType Type) : IChangeInfo, IAction;
