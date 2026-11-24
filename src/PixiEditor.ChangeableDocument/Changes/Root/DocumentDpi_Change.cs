using PixiEditor.ChangeableDocument.ChangeInfos.Root;

namespace PixiEditor.ChangeableDocument.Changes.Root;

internal class DocumentDpi_Change : Change
{
    private float targetDpi;
    private float old;

    [GenerateMakeChangeAction]
    public DocumentDpi_Change(float dpi)
    {
        targetDpi = dpi;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if(targetDpi <= 0)
            return false;

        old = target.Dpi;
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        target.Dpi = targetDpi;
        ignoreInUndo = Math.Abs(old - targetDpi) < float.Epsilon;

        return new DocumentDpi_ChangeInfo(targetDpi);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.Dpi = old;
        return new DocumentDpi_ChangeInfo(old);
    }
}
