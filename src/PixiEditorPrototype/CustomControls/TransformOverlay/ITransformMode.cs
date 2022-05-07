using System.Windows.Media;
using ChunkyImageLib.DataHolders;

namespace PixiEditorPrototype.CustomControls.TransformOverlay;
internal interface ITransformMode
{
    void OnRender(DrawingContext context);
    void OnAnchorDrag(Vector2d newPos, Anchor anchor);
    Anchor? GetAnchorInPosition(Vector2d pos);
}
