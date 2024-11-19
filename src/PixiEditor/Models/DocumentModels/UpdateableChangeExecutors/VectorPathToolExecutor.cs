using Avalonia.Media;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Controllers.InputDevice;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.Models.Tools;
using Color = Drawie.Backend.Core.ColorsImpl.Color;
using Colors = Drawie.Backend.Core.ColorsImpl.Colors;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

internal class VectorPathToolExecutor : UpdateableChangeExecutor
{
    private IStructureMemberHandler member;
    private VectorPath startingPath;
    private IVectorPathToolHandler vectorPathToolHandler;
    private IBasicShapeToolbar toolbar;

    public override ExecutionState Start()
    {
        vectorPathToolHandler = GetHandler<IVectorPathToolHandler>();

        member = document.SelectedStructureMember;

        if (member is null)
        {
            return ExecutionState.Error;
        }

        toolbar = (IBasicShapeToolbar)vectorPathToolHandler.Toolbar;

        if (member is IVectorLayerHandler vectorLayerHandler)
        {
            var shapeData = vectorLayerHandler.GetShapeData(document.AnimationHandler.ActiveFrameTime);
            if (shapeData is PathVectorData pathData)
            {
                startingPath = pathData.Path;
            }
            else if (shapeData is null)
            {
                startingPath = new VectorPath();
            }
            else
            {
                return ExecutionState.Error;
            }

            startingPath.MoveTo((VecF)controller.LastPrecisePosition);
            internals.ActionAccumulator.AddActions(new SetShapeGeometry_Action(member.Id, ConstructShapeData()));

            document.PathOverlayHandler.Show(startingPath);
        }

        return ExecutionState.Success;
    }

    public override void OnLeftMouseButtonDown(MouseOnCanvasEventArgs args)
    {
        startingPath.LineTo((VecF)args.PositionOnCanvas);
        PathVectorData vectorData = ConstructShapeData();

        internals.ActionAccumulator.AddActions(new SetShapeGeometry_Action(member.Id, vectorData));
    }


    public override void OnColorChanged(Color color, bool primary)
    {
        if (primary && toolbar.SyncWithPrimaryColor)
        {
            toolbar.StrokeColor = color.ToColor();
            toolbar.FillColor = color.ToColor();
        }
    }

    public override void OnSettingsChanged(string name, object value)
    {
        internals.ActionAccumulator.AddActions(new SetShapeGeometry_Action(member.Id, ConstructShapeData()));
    }

    public override void ForceStop()
    {
        internals.ActionAccumulator.AddActions(new EndSetShapeGeometry_Action());
    }

    private PathVectorData ConstructShapeData()
    {
        return new PathVectorData(new VectorPath(startingPath))
        {
            StrokeWidth = toolbar.ToolSize,
            StrokeColor = toolbar.StrokeColor.ToColor(),
            FillColor = toolbar.Fill ? toolbar.FillColor.ToColor() : Colors.Transparent,
        };
    }
}
