using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;
internal class BasicPen_UpdateableChange : UpdateableChange
{
    private readonly Guid memberGuid;
    private readonly int strokeWidth;
    private readonly SKColor color;
    private readonly bool drawOnMask;
    private readonly List<VecI> points = new();

    [GenerateUpdateableChangeActions]
    public BasicPen_UpdateableChange(Guid memberGuid, int strokeWidth, SKColor color, VecI point, bool drawOnMask)
    {
        this.memberGuid = memberGuid;
        this.strokeWidth = strokeWidth;
        this.color = color;
        this.drawOnMask = drawOnMask;
        points.Add(point);
    }

    [UpdateChangeMethod]
    public void Update(VecI point)
    {
        points.Add(point);
    }

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        if (!DrawingChangeHelper.IsValidForDrawing(target, memberGuid, drawOnMask))
            return new Error();
        return new Success();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, out bool ignoreInUndo)
    {
        throw new NotImplementedException();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        throw new NotImplementedException();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        throw new NotImplementedException();
    }
}
