using Avalonia.Input;
using Avalonia.Media;
using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changes.Vectors;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Controllers.InputDevice;
using PixiEditor.Models.DocumentModels.Public;
using PixiEditor.Models.DocumentModels.UpdateableChangeExecutors.Features;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.Models.Tools;
using PixiEditor.ViewModels.Tools.Tools;
using PixiEditor.ViewModels.Tools.ToolSettings.Settings;
using PixiEditor.Views.Overlays.PathOverlay;
using Color = Drawie.Backend.Core.ColorsImpl.Color;
using Colors = Drawie.Backend.Core.ColorsImpl.Colors;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

internal class VectorPathToolExecutor : UpdateableChangeExecutor, IPathExecutorFeature
{
    private IStructureMemberHandler member;
    private VectorPath startingPath;
    private IVectorPathToolHandler vectorPathToolHandler;
    private IFillableShapeToolbar toolbar;
    private IColorsHandler colorHandler;
    private bool isValidPathLayer;
    private IDisposable restoreSnapping;

    public override ExecutorType Type => ExecutorType.ToolLinked;

    public bool StopExecutionOnNormalUndo => false;

    public override bool BlocksOtherActions => false;

    private bool mouseDown;

    public override ExecutionState Start()
    {
        vectorPathToolHandler = GetHandler<IVectorPathToolHandler>();

        member = document.SelectedStructureMember;

        if (member is null)
        {
            return ExecutionState.Error;
        }

        toolbar = (IFillableShapeToolbar)vectorPathToolHandler.Toolbar;
        colorHandler = GetHandler<IColorsHandler>();

        if (member is IVectorLayerHandler vectorLayerHandler)
        {
            var shapeData = vectorLayerHandler.GetShapeData(document.AnimationHandler.ActiveFrameTime);
            bool wasNull = false;
            isValidPathLayer = true;
            if (shapeData is PathVectorData pathData)
            {
                startingPath = new VectorPath(pathData.Path);
                ApplySettings(pathData);
                startingPath.Transform(pathData.TransformationMatrix);
            }
            else if (shapeData is null)
            {
                wasNull = true;
                startingPath = new VectorPath();
            }
            else
            {
                isValidPathLayer = false;
                return ExecutionState.Success;
            }

            document.PathOverlayHandler.Show(startingPath, false, AddToUndo);
            if (controller.LeftMousePressed)
            {
                var snapped =
                    document.SnappingHandler.SnappingController.GetSnapPoint(controller.LastPrecisePosition, out _,
                        out _);
                if (wasNull)
                {
                    startingPath.MoveTo((VecF)snapped);
                }

                if (toolbar.SyncWithPrimaryColor)
                {
                    toolbar.StrokeBrush = new SolidColorBrush(colorHandler.PrimaryColor.ToColor());
                    toolbar.FillBrush = new SolidColorBrush(colorHandler.PrimaryColor.ToColor());
                }

                //below forces undo before starting new path
                // internals.ActionAccumulator.AddFinishedActions(new EndSetShapeGeometry_Action());

                // internals.ActionAccumulator.AddActions(new SetShapeGeometry_Action(member.Id, ConstructShapeData(startingPath)));
            }
        }
        else
        {
            return ExecutionState.Error;
        }

        restoreSnapping = SimpleShapeToolExecutor.DisableSelfSnapping(member.Id, document);
        return ExecutionState.Success;
    }

    public override void OnPrecisePositionChange(VecD pos)
    {
        if (mouseDown)
        {
            return;
        }

        VecD mouseSnap =
            document.SnappingHandler.SnappingController.GetSnapPoint(pos, out string snapXAxis,
                out string snapYAxis);
        HighlightSnapping(snapXAxis, snapYAxis);

        if (!string.IsNullOrEmpty(snapXAxis) || !string.IsNullOrEmpty(snapYAxis))
        {
            document.SnappingHandler.SnappingController.HighlightedPoint = mouseSnap;
        }
        else
        {
            document.SnappingHandler.SnappingController.HighlightedPoint = null;
        }
    }

    public override void OnLeftMouseButtonDown(MouseOnCanvasEventArgs args)
    {
        if (args.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            Guid? created =
                document.Operations.CreateStructureMember(typeof(VectorLayerNode), ActionSource.Automated);

            if (created is null) return;

            document.Operations.SetSelectedMember(created.Value);
        }
    }

    private bool WholePathClosed()
    {
        EditableVectorPath editablePath = new EditableVectorPath(startingPath);

        return editablePath.SubShapes.Count > 0 && editablePath.SubShapes.All(x => x.IsClosed);
    }

    public override void OnLeftMouseButtonUp(VecD pos)
    {
        mouseDown = false;
    }

    public override void OnColorChanged(Color color, bool primary)
    {
        if (primary && toolbar.SyncWithPrimaryColor)
        {
            toolbar.StrokeBrush = new SolidColorBrush(color.ToColor());
            toolbar.FillBrush = new SolidColorBrush(color.ToColor());
        }
    }

    public override void OnSettingsChanged(string name, object value)
    {
        if (document.PathOverlayHandler.IsActive)
        {
            VectorShapeChangeType changeType = name switch
            {
                nameof(IFillableShapeToolbar.Fill) => VectorShapeChangeType.Fill,
                nameof(IShapeToolbar.StrokeBrush) => VectorShapeChangeType.Stroke,
                nameof(IShapeToolbar.ToolSize) => VectorShapeChangeType.Stroke,
                nameof(IShapeToolbar.AntiAliasing) => VectorShapeChangeType.OtherVisuals,
                _ => VectorShapeChangeType.All
            };

            internals.ActionAccumulator.AddFinishedActions(
                new SetShapeGeometry_Action(member.Id, ConstructShapeData(startingPath), changeType),
                new EndSetShapeGeometry_Action());
        }
    }

    public override void ForceStop()
    {
        document.PathOverlayHandler.Hide();

        restoreSnapping?.Dispose();

        HighlightSnapping(null, null);
        internals.ActionAccumulator.AddFinishedActions(new EndSetShapeGeometry_Action());
    }

    private void AddToUndo(VectorPath path)
    {
        internals.ActionAccumulator.AddFinishedActions(new EndSetShapeGeometry_Action(),
            new SetShapeGeometry_Action(member.Id, ConstructShapeData(path), VectorShapeChangeType.GeometryData),
            new EndSetShapeGeometry_Action());
    }

    private PathVectorData ConstructShapeData(VectorPath? path)
    {
        if (path is null)
        {
            return new PathVectorData(new VectorPath() { FillType = (PathFillType)vectorPathToolHandler.FillMode })
            {
                StrokeWidth = (float)toolbar.ToolSize,
                Stroke = toolbar.StrokeBrush.ToPaintable(),
                FillPaintable = toolbar.Fill ? toolbar.FillBrush.ToPaintable() : Colors.Transparent,
                Fill = toolbar.Fill,
                StrokeLineCap = vectorPathToolHandler.StrokeLineCap,
                StrokeLineJoin = vectorPathToolHandler.StrokeLineJoin
            };
        }

        return new PathVectorData(new VectorPath(path) { FillType = (PathFillType)vectorPathToolHandler.FillMode })
        {
            StrokeWidth = (float)toolbar.ToolSize,
            Stroke = toolbar.StrokeBrush.ToPaintable(),
            FillPaintable = toolbar.Fill ? toolbar.FillBrush.ToPaintable() : Colors.Transparent,
            Fill = toolbar.Fill,
            StrokeLineCap = vectorPathToolHandler.StrokeLineCap,
            StrokeLineJoin = vectorPathToolHandler.StrokeLineJoin
        };
    }

    public void OnPathChanged(VectorPath path)
    {
        if (document.PathOverlayHandler.IsActive)
        {
            startingPath = path;
            internals.ActionAccumulator.AddActions(new SetShapeGeometry_Action(member.Id,
                ConstructShapeData(startingPath), VectorShapeChangeType.GeometryData));
        }
    }

    public bool IsFeatureEnabled<T>()
    {
        Type feature = typeof(T);
        return feature == typeof(IMidChangeUndoableExecutor)
               || feature == typeof(ITransformableExecutor)
               || feature == typeof(IPathExecutorFeature);
    }

    protected void HighlightSnapping(string? snapX, string? snapY)
    {
        document!.SnappingHandler.SnappingController.HighlightedXAxis = snapX;
        document!.SnappingHandler.SnappingController.HighlightedYAxis = snapY;
        document.SnappingHandler.SnappingController.HighlightedPoint = null;
    }

    private void ApplySettings(PathVectorData pathData)
    {
        toolbar.ToolSize = pathData.StrokeWidth;
        toolbar.StrokeBrush = pathData.Stroke.ToBrush();
        toolbar.ToolSize = pathData.StrokeWidth;
        toolbar.Fill = pathData.Fill;
        toolbar.FillBrush = pathData.FillPaintable.ToBrush();
        toolbar.GetSetting<EnumSettingViewModel<VectorPathFillType>>(nameof(VectorPathToolViewModel.FillMode)).Value =
            (VectorPathFillType)pathData.Path.FillType;
        toolbar.GetSetting<EnumSettingViewModel<StrokeCap>>(nameof(VectorPathToolViewModel.StrokeLineCap)).Value =
            pathData.StrokeLineCap;
        toolbar.GetSetting<EnumSettingViewModel<StrokeJoin>>(nameof(VectorPathToolViewModel.StrokeLineJoin)).Value =
            pathData.StrokeLineJoin;
    }
}
