using Avalonia.Media;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Text;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Changes.Vectors;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Controllers.InputDevice;
using PixiEditor.Models.DocumentModels.UpdateableChangeExecutors.Features;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using Color = Drawie.Backend.Core.ColorsImpl.Color;
using Colors = Drawie.Backend.Core.ColorsImpl.Colors;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

internal class VectorTextToolExecutor : UpdateableChangeExecutor, ITextOverlayEvents, IQuickToolSwitchable,
    IQuickColorLayerExecutor
{
    private ITextToolHandler textHandler;
    private ITextToolbar toolbar;
    private IStructureMemberHandler selectedMember;

    private RichText lastText;
    private VecD position;
    private Matrix3X3 lastMatrix = Matrix3X3.Identity;
    private bool isListeningForValidLayer;
    private VectorPath? onPath;

    private VecD clickPos;
    private bool wasDrawingSize;

    public override bool BlocksOtherActions => false;

    public override ExecutorType Type => ExecutorType.ToolLinked;

    public override ExecutionState Start()
    {
        textHandler = GetHandler<ITextToolHandler>();
        if (textHandler == null)
        {
            return ExecutionState.Error;
        }

        toolbar = textHandler.Toolbar as ITextToolbar;
        if (toolbar == null)
        {
            return ExecutionState.Error;
        }

        selectedMember = document.SelectedStructureMember;

        if (selectedMember is not IVectorLayerHandler layerHandler)
        {
            isListeningForValidLayer = true;
            return ExecutionState.Success;
        }

        IColorsHandler colorsVM = GetHandler<IColorsHandler>();

        isListeningForValidLayer = false;
        var shape = layerHandler.GetShapeData(document.AnimationHandler.ActiveFrameBindable);

        if (toolbar.SyncWithPrimaryColor)
        {
            toolbar.FillBrush = new SolidColorBrush(colorsVM.PrimaryColor.ToColor());
        }

        if (shape is TextVectorData textData)
        {
            document.TextOverlayHandler.Show(textData.Text, textData.Position, textData.TransformationMatrix);

            UpdateToolbar(textData);

            onPath = textData.Path;
            lastText = textData.Text.Clone();
            position = textData.Position;
            lastMatrix = textData.TransformationMatrix;
        }
        else if (shape is null)
        {
            RichText newEmpty = new RichText(string.Empty, toolbar.ConstructFont())
            {
                Spacing = 12,
            };

            newEmpty.Inlines[0].Fill = toolbar.Fill;
            newEmpty.Inlines[0].FillPaintable = toolbar.FillBrush.ToPaintable();
            newEmpty.Inlines[0].StrokeWidth = (float)toolbar.ToolSize;
            newEmpty.Inlines[0].StrokePaintable = toolbar.StrokeBrush.ToPaintable();

            document.TextOverlayHandler.Show(newEmpty, controller.LastPrecisePosition,
                Matrix3X3.Identity);
            position = controller.LastPrecisePosition;
            clickPos = controller.LastPrecisePosition;

            lastText = newEmpty.Clone();
            // TODO: Implement proper putting on path editing
            /*if (controller.LeftMousePressed)
            {
                TryPutOnPath(controller.LastPrecisePosition);
            }*/
        }
        else
        {
            return ExecutionState.Error;
        }

        return ExecutionState.Success;
    }

    private void UpdateToolbar(TextVectorData textData)
    {
        toolbar.Fill = textData.Fill;
        toolbar.FillBrush = textData.FillPaintable.ToBrush();
        toolbar.StrokeBrush = textData.Stroke.ToBrush();
        toolbar.ToolSize = textData.StrokeWidth;
        UpdateInlineSettings(textData.Text);
    }

    private void UpdateInlineSettings(RichText text)
    {
        var activeInline = GetActiveInline(text);
        toolbar.FontFamily = activeInline.Font.Family;
        toolbar.FontSize = activeInline.Font.Size;
        toolbar.Spacing = activeInline.LineHeight;
        toolbar.FontFamily = activeInline.Font.Family;
        toolbar.FontSize = activeInline.Font.Size;
        toolbar.Bold = activeInline.Font.Bold;
        toolbar.Italic = activeInline.Font.Italic;
        toolbar.Fill = activeInline.Fill;
        toolbar.FillBrush = activeInline.FillPaintable.ToBrush();
        toolbar.StrokeBrush = activeInline.StrokePaintable.ToBrush();
        toolbar.ToolSize = activeInline.StrokeWidth;
    }

    private TextInline GetActiveInline(RichText text)
    {
        int cursorPos = document.TextOverlayHandler.CursorPosition;

        return text.GetInlineAt(cursorPos, out _, out _);
    }

    public override void OnLeftMouseButtonDown(MouseOnCanvasEventArgs args)
    {
        var topMostWithinClick = QueryLayers<IVectorLayerHandler>(args.Point.PositionOnCanvas);

        clickPos = args.Point.PositionOnCanvas;
        var firstLayer = topMostWithinClick.FirstOrDefault();
        args.Handled = firstLayer != null;
        if (firstLayer is not IVectorLayerHandler layerHandler ||
            layerHandler.GetShapeData(document.AnimationHandler.ActiveFrameTime) is not TextVectorData)
        {
            args.Handled = false;
            if (document.TextOverlayHandler.IsActive)
            {
                args.Handled = true;
                document.TextOverlayHandler.Hide();
            }

            return;
        }

        document.Operations.SetSelectedMember(layerHandler.Id);
        document.Operations.InvokeCustomAction(
            () =>
            {
                if (!document.TextOverlayHandler.IsActive)
                {
                    document.TextOverlayHandler.Show(lastText, position, lastMatrix);
                }

                document.TextOverlayHandler.SetCursorPosition(args.Point.PositionOnCanvas);
            }, false);
    }

    public override void OnPrecisePositionChange(MouseOnCanvasEventArgs args)
    {
        if (document.TextOverlayHandler.IsActive && internals.ChangeController.LeftMousePressed &&
            lastText.RawText == null)
        {
            double distance = Math.Abs(clickPos.Y - args.Point.PositionOnCanvas.Y);
            if (!wasDrawingSize && distance < 10) return;
            wasDrawingSize = true;
            position = new VecD(position.X, args.Point.PositionOnCanvas.Y);
            document.TextOverlayHandler.Position = position;
            document.TextOverlayHandler.PreviewSize = true;
            var textData = ConstructTextData(lastText);
            toolbar.FontSize = distance * RichText.PtToPx;
            internals.ActionAccumulator.AddActions(new SetShapeGeometry_Action(selectedMember.Id, textData,
                VectorShapeChangeType.GeometryData));
        }
    }

    public override void OnLeftMouseButtonUp(VecD pos)
    {
        if (wasDrawingSize)
        {
            document.TextOverlayHandler.PreviewSize = false;
        }
    }

    public void OnQuickToolSwitch()
    {
        document.TextOverlayHandler.SetCursorPosition(internals.ChangeController.LastPrecisePosition);
    }

    public override void ForceStop()
    {
        internals.ActionAccumulator.AddFinishedActions(new EndSetShapeGeometry_Action());
        document?.TextOverlayHandler?.Hide();

        TextVectorData? previousData = null;
        if (selectedMember != null && document?.AnimationHandler != null)
        {
            previousData =
                (document.StructureHelper.Find(selectedMember.Id) as IVectorLayerHandler)?
                .GetShapeData(document.AnimationHandler.ActiveFrameTime) as TextVectorData;
        }
    }

    public void OnTextChanged(RichText text)
    {
        if (text == lastText)
        {
            return;
        }

        var constructedText = ConstructTextData(text);
        internals.ActionAccumulator.AddFinishedActions(
            new SetShapeGeometry_Action(selectedMember.Id, constructedText, VectorShapeChangeType.GeometryData),
            new EndSetShapeGeometry_Action(),
            new SetLowDpiRendering_Action(selectedMember.Id, toolbar.ForceLowDpiRendering));
        lastText = text;
    }

    public void OnSelectionChanged(int cursorPosition, int selectionEnd)
    {
        UpdateInlineSettings(lastText);
    }

    public override void OnSettingsChanged(string name, object value)
    {
        if (!document.TextOverlayHandler.IsActive) return;

        if (isListeningForValidLayer)
        {
            return;
        }

        int? currentlyEditingInlineIndex = document.TextOverlayHandler.CurrentlyEditingInlineIndex;

        if (currentlyEditingInlineIndex == null || currentlyEditingInlineIndex < 0 ||
            currentlyEditingInlineIndex >= lastText.Inlines.Count)
        {
            return;
        }

        var text = lastText.Clone();
        var currentInline = lastText.Inlines[currentlyEditingInlineIndex.Value];
        int editingIndex = currentlyEditingInlineIndex.Value;

        if (document.TextOverlayHandler.CursorPosition != document.TextOverlayHandler.SelectionEnd)
        {
            currentInline = text.SplitInline(currentlyEditingInlineIndex.Value,
                document.TextOverlayHandler.CursorPosition,
                document.TextOverlayHandler.SelectionEnd);
            editingIndex = text.IndexOfInline(currentInline);
        }

        if (name == nameof(ITextToolbar.FontFamily))
        {
            currentInline.Font = currentInline.Font with { Family = (FontFamilyName)value };
        }
        else if (name == nameof(ITextToolbar.FontSize))
        {
            currentInline.Font = currentInline.Font with { Size = (double)value };
        }
        else if (name == nameof(ITextToolbar.Fill))
        {
            currentInline.Fill = (bool)value;
        }
        else if (name == nameof(ITextToolbar.FillBrush))
        {
            currentInline.FillPaintable = ((IBrush)value).ToPaintable();
        }
        else if (name == nameof(ITextToolbar.ToolSize))
        {
            currentInline.StrokeWidth = (float)(double)value;
        }
        else if (name == nameof(ITextToolbar.StrokeBrush))
        {
            currentInline.StrokePaintable = ((IBrush)value).ToPaintable();
        }
        else if (name == nameof(ITextToolbar.Spacing))
        {
            text.Spacing = (double)value;
        }
        else if (name == nameof(ITextToolbar.Bold))
        {
            currentInline.Font = currentInline.Font with { Bold = (bool)value };
        }
        else if (name == nameof(ITextToolbar.Italic))
        {
            currentInline.Font = currentInline.Font with { Italic = (bool)value };
        }

        text.UpdateInline(editingIndex, currentInline);

        currentInline = text.MergeAdjacentInlines(currentInline);

        VectorShapeChangeType changeType = name switch
        {
            nameof(ITextToolbar.Fill) => VectorShapeChangeType.Fill,
            nameof(ITextToolbar.FillBrush) => VectorShapeChangeType.Fill,
            nameof(ITextToolbar.StrokeBrush) => VectorShapeChangeType.Stroke,
            nameof(ITextToolbar.ToolSize) => VectorShapeChangeType.GeometryData,
            nameof(ITextToolbar.Spacing) => VectorShapeChangeType.GeometryData,
            nameof(ITextToolbar.AntiAliasing) => VectorShapeChangeType.OtherVisuals,
            nameof(ITextToolbar.ForceLowDpiRendering) => VectorShapeChangeType.OtherVisuals,
            _ => VectorShapeChangeType.OtherVisuals
        };

        var constructedText = ConstructTextData(text);
        /*var layer = document.StructureHelper.Find(selectedMember.Id);
        FontEdging previousEdging = constructedText.Font.Edging;
        bool previousAntiAlias = constructedText.AntiAlias;
        bool previousSubpixel = constructedText.Font.SubPixel;

        constructedText.AntiAlias = previousAntiAlias;
        constructedText.Font = constructedText.Font with { Edging = previousEdging, SubPixel = previousSubpixel };*/

        internals.ActionAccumulator.AddActions(
            new SetShapeGeometry_Action(selectedMember.Id, constructedText, changeType),
            new SetLowDpiRendering_Action(selectedMember.Id, toolbar.ForceLowDpiRendering));

        /*
        document.TextOverlayHandler.Font = default; // Forces refreshing glyphs
        document.TextOverlayHandler.Font = constructedText.Font;
        document.TextOverlayHandler.Spacing = toolbar.Spacing;*/
        document.TextOverlayHandler.Text = text;
        lastText = text;
    }

    public override void OnColorChanged(Color color, bool primary)
    {
        if (!primary || !toolbar.SyncWithPrimaryColor)
        {
            return;
        }

        toolbar.StrokeBrush = new SolidColorBrush(color.ToColor());
        toolbar.FillBrush = new SolidColorBrush(color.ToColor());
    }

    private void TryPutOnPath(VecD pos)
    {
        var topMostWithinClick = QueryLayers<IVectorLayerHandler>(pos);
        var firstValidLayer = topMostWithinClick.FirstOrDefault(x =>
            x.GetShapeData(document.AnimationHandler.ActiveFrameTime) is not null and not TextVectorData);

        if (firstValidLayer is null)
        {
            return;
        }

        var shape = firstValidLayer.GetShapeData(document.AnimationHandler.ActiveFrameTime);

        ShapeVectorData newShape = (ShapeVectorData)(shape as ShapeVectorData).Clone();

        newShape.Fill = false;
        newShape.StrokeWidth = 0;

        onPath = newShape.ToPath();

        var constructedText = ConstructTextData(lastText);
        internals.ActionAccumulator.AddFinishedActions(
            new SetShapeGeometry_Action(selectedMember.Id, constructedText, VectorShapeChangeType.GeometryData),
            new EndSetShapeGeometry_Action(),
            new SetLowDpiRendering_Action(selectedMember.Id, toolbar.ForceLowDpiRendering),
            new SetShapeGeometry_Action(firstValidLayer.Id, newShape, VectorShapeChangeType.GeometryData),
            new EndSetShapeGeometry_Action());
    }

    private TextVectorData ConstructTextData(RichText text)
    {
        return new TextVectorData()
        {
            Text = text,
            Position = position,
            Fill = toolbar.Fill, // TODO per inline
            FillPaintable = toolbar.FillBrush.ToPaintable(), // TODO per inline
            StrokeWidth = (float)toolbar.ToolSize, // TODO per inline
            Stroke = toolbar.StrokeBrush.ToPaintable(), // TODO per inline
            TransformationMatrix = lastMatrix,
            Spacing = toolbar.Spacing,
            AntiAlias = toolbar.AntiAliasing,
            Path = onPath,
            // TODO: MaxWidth = toolbar.MaxWidth
        };
    }

    bool IExecutorFeature.IsFeatureEnabled<T>()
    {
        return typeof(T) == typeof(ITextOverlayEvents) || typeof(T) == typeof(IQuickToolSwitchable) ||
               (typeof(T) == typeof(IQuickColorLayerExecutor) && document.TextOverlayHandler.IsActive);
    }

    public void EndQuickColorChange()
    {
    }
}
