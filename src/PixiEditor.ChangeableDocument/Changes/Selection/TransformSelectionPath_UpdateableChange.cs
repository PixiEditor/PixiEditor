using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Selection;
internal class TransformSelectionPath_UpdateableChange : UpdateableChange
{
    private VectorPath? originalPath;
    private RectD originalTightBounds;
    private ShapeCorners newCorners;

    [GenerateUpdateableChangeActions]
    public TransformSelectionPath_UpdateableChange(ShapeCorners corners)
    {
        this.newCorners = corners;
    }

    [UpdateChangeMethod]
    public void Update(ShapeCorners corners)
    {
        this.newCorners = corners;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (target.Selection.SelectionPath.IsEmpty)
            return false;
        originalPath = new(target.Selection.SelectionPath);
        originalTightBounds = originalPath.TightBounds;
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        ignoreInUndo = false;
        return SelectionChangeHelper.DoSelectionTransform(target, originalPath!, originalTightBounds, newCorners);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        return SelectionChangeHelper.DoSelectionTransform(target, originalPath!, originalTightBounds, newCorners);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var toDispose = target.Selection.SelectionPath;
        target.Selection.SelectionPath = new VectorPath(originalPath!);
        toDispose.Dispose();
        return new Selection_ChangeInfo(new VectorPath(target.Selection.SelectionPath));
    }

    public override void Dispose()
    {
        originalPath?.Dispose();
    }
}
