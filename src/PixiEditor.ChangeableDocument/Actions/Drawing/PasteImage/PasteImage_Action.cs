using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Changes;
using PixiEditor.ChangeableDocument.Changes.Drawing;

namespace PixiEditor.ChangeableDocument.Actions.Drawing.PasteImage;
public record class PasteImage_Action : IStartOrUpdateChangeAction
{
    public PasteImage_Action(Surface image, ShapeCorners corners, Guid layerGuid, bool isDrawingOnMask)
    {
        Image = image;
        Corners = corners;
        GuidValue = layerGuid;
        IsDrawingOnMask = isDrawingOnMask;
    }

    public Surface Image { get; }
    public ShapeCorners Corners { get; }
    public Guid GuidValue { get; }
    public bool IsDrawingOnMask { get; }

    UpdateableChange IStartOrUpdateChangeAction.CreateCorrespondingChange()
    {
        return new PasteImage_UpdateableChange(Corners, Image, GuidValue, IsDrawingOnMask);
    }

    void IStartOrUpdateChangeAction.UpdateCorrespodingChange(UpdateableChange change)
    {
        ((PasteImage_UpdateableChange)change).Update(Corners);
    }
}
