using System.Collections.Immutable;
using PixiEditor.ChangeableDocument.ChangeInfos.Root.ReferenceLayerChangeInfos;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Root.ReferenceLayerChanges;

internal class SetReferenceLayer_Change : Change
{
    private readonly ImmutableArray<byte> imagePbgra32Bytes;
    private readonly VecI imageSize;
    private readonly ShapeCorners shape;

    private ReferenceLayer? lastReferenceLayer;
    
    [GenerateMakeChangeAction]
    public SetReferenceLayer_Change(ShapeCorners shape, ImmutableArray<byte> imagePbgra32Bytes, VecI imageSize)
    {
        this.imagePbgra32Bytes = imagePbgra32Bytes;
        this.imageSize = imageSize;
        this.shape = shape;
    }

    public override bool InitializeAndValidate(Document target)
    {
        lastReferenceLayer = target.ReferenceLayer?.Clone();
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        target.ReferenceLayer = new ReferenceLayer(imagePbgra32Bytes, imageSize, shape);
        ignoreInUndo = false;
        return new SetReferenceLayer_ChangeInfo(imagePbgra32Bytes, imageSize, shape);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.ReferenceLayer = lastReferenceLayer?.Clone();
        if (lastReferenceLayer is null)
            return new DeleteReferenceLayer_ChangeInfo();
        return new SetReferenceLayer_ChangeInfo(
            lastReferenceLayer.ImagePbgra32Bytes,
            lastReferenceLayer.ImageSize,
            lastReferenceLayer.Shape);
    }
}
