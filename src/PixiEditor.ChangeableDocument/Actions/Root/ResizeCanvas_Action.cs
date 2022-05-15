using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Changes;
using PixiEditor.ChangeableDocument.Changes.Root;

namespace PixiEditor.ChangeableDocument.Actions.Root;

public record class ResizeCanvas_Action : IMakeChangeAction
{
    public VecI Size { get; }
    public ResizeCanvas_Action(VecI size)
    {
        Size = size;
    }
    Change IMakeChangeAction.CreateCorrespondingChange()
    {
        return new ResizeCanvas_Change(Size);
    }
}
