namespace PixiEditor.ChangeableDocument;

public struct BudgetedCall
{
    public DateTime MustCompleteWorkBefore { get; }

    public BudgetedCall(DateTime mustCompleteWorkBefore)
    {
        MustCompleteWorkBefore = mustCompleteWorkBefore;
    }

    public bool Exceeded()
    {
        return DateTime.Now >= MustCompleteWorkBefore;
    }
}
