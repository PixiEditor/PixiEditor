using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Changes;

namespace PixiEditor.ChangeableDocument.Actions.Document
{
    public record class ResizeCanvas_Action : IMakeChangeAction
    {
        public Vector2i Size { get; }
        public ResizeCanvas_Action(Vector2i size)
        {
            Size = size;
        }
        Change IMakeChangeAction.CreateCorrespondingChange()
        {
            return new ResizeCanvas_Change(Size);
        }
    }
}
