using PixiEditor.ChangeableDocument.ChangeInfos.Structure;

namespace PixiEditor.ChangeableDocument.Changes.Root;

internal class CreateReferenceLayer_UpdateableChange : UpdateableChange
{
    private readonly Surface? surface;
    private ShapeCorners shape;

    [GenerateUpdateableChangeActions]
    public CreateReferenceLayer_UpdateableChange(Surface? surface, ShapeCorners shape)
    {
        this.surface = surface;
        this.shape = shape;
    }

    [UpdateChangeMethod]
    public void Update(ShapeCorners shape)
    {
        this.shape = shape;
    }

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        if (surface is null)
        {
            return new Error();
        }

        target.ReferenceLayer = new ReferenceLayer(surface!, shape);
        return new Success();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, out bool ignoreInUndo)
    {
        target.ReferenceLayer!.Shape = shape;
        ignoreInUndo = true;
        return new CreateReferenceLayer_ChangeInfo(true);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        throw new NotImplementedException();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        target.ReferenceLayer!.Shape = shape;
        return new CreateReferenceLayer_ChangeInfo(true);
    }
}
