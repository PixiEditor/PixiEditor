using Drawie.Backend.Core.Numerics;
using PixiEditor.ChangeableDocument.ChangeInfos.Root.ReferenceLayerChangeInfos;

namespace PixiEditor.ChangeableDocument.Changes.Root.ReferenceLayerChanges;

internal class TransformReferenceLayer_UpdateableChange : UpdateableChange
{
    private ShapeCorners originalCorners;
    private ShapeCorners corners;

    [GenerateUpdateableChangeActions]
    public TransformReferenceLayer_UpdateableChange(ShapeCorners corners)
    {
        this.corners = corners;
    }

    [UpdateChangeMethod]
    public void Update(ShapeCorners corners)
    {
        this.corners = corners;
    }
    
    public override bool InitializeAndValidate(Document target)
    {
        if (target.ReferenceLayer is null)
            return false;
        originalCorners = target.ReferenceLayer.Shape;
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        target.ReferenceLayer!.Shape = corners;
        return new TransformReferenceLayer_ChangeInfo(corners);
    }
    
    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        target.ReferenceLayer!.Shape = corners;
        ignoreInUndo = false;
        return new TransformReferenceLayer_ChangeInfo(corners);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.ReferenceLayer!.Shape = originalCorners;
        return new TransformReferenceLayer_ChangeInfo(originalCorners);
    }
}
