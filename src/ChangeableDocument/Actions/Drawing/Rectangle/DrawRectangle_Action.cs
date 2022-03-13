using ChangeableDocument.Changes;
using ChangeableDocument.Changes.Drawing;
using ChunkyImageLib.DataHolders;

namespace ChangeableDocument.Actions.Drawing.Rectangle
{
    public record struct DrawRectangle_Action : IStartOrUpdateChangeAction
    {
        public DrawRectangle_Action(Guid layerGuid, ShapeData rectangle)
        {
            LayerGuid = layerGuid;
            Rectangle = rectangle;
        }

        public Guid LayerGuid { get; }
        public ShapeData Rectangle { get; }

        void IStartOrUpdateChangeAction.UpdateCorrespodingChange(IUpdateableChange change)
        {
            ((DrawRectangle_UpdateableChange)change).Update(Rectangle);
        }

        IUpdateableChange IStartOrUpdateChangeAction.CreateCorrespondingChange()
        {
            return new DrawRectangle_UpdateableChange(LayerGuid, Rectangle);
        }
    }
}
