using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Numerics;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

internal class VectorEllipseToolExecutor : ShapeToolExecutor<IVectorEllipseToolHandler>
{
    protected override void DrawShape(VecI currentPos, double rotationRad, bool firstDraw)
    {
        throw new NotImplementedException();
    }

    protected override IAction SettingsChangedAction()
    {
        throw new NotImplementedException();
    }

    protected override IAction TransformMovedAction(ShapeData data, ShapeCorners corners)
    {
        throw new NotImplementedException();
    }

    protected override IAction EndDrawAction()
    {
        throw new NotImplementedException();
    }
}
