namespace PixiEditor.ChangeableDocument.Changes.Root;

internal class ClipCanvas_Change : UpdateableChange
{
    public override bool InitializeAndValidate(Document target)
    {
        throw new NotImplementedException();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        throw new NotImplementedException();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        throw new NotImplementedException();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        throw new NotImplementedException();
    }
}
