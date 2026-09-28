using PixiEditor.ChangeableDocument.Actions;

namespace PixiEditor.ChangeableDocument.Changes;

internal interface IBudgetedUpdateableChange
{
    BudgetedCall? Budget { get; set; }
    bool HasUnfinishedWork { get; }
    void EnsureAllWorkDone(Document document);
    IAction? GetIncrementWorkAction();
}
