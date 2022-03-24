namespace ChangeableDocument.Actions.Undo
{
    public record class MergeLatestChanges_Action : IAction
    {
        public MergeLatestChanges_Action(int count)
        {
            Count = count;
        }

        public int Count { get; }
    }
}
