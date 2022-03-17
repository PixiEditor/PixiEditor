using ChangeableDocument.Changes;
using ChunkyImageLib.DataHolders;

namespace ChangeableDocument.Actions.Document
{
    public record class ResizeCanvas_Action : IMakeChangeAction
    {
        public Vector2i Size { get; }
        public ResizeCanvas_Action(Vector2i size)
        {
            Size = size;
        }
        IChange IMakeChangeAction.CreateCorrespondingChange()
        {
            return new ResizeCanvas_Change(Size);
        }
    }
}
