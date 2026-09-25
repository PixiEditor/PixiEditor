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
        IReadOnlyList<TextInline> inlines = GetEditingInlines(text);

        if (inlines.Count == 0)
            return;

        TextInline first = inlines[0];

        if (inlines.All(x => x.Font.Family.Equals(first.Font.Family)))
            toolbar.FontFamily = first.Font.Family;

        if (inlines.All(x => Math.Abs(x.Font.Size - first.Font.Size) < float.Epsilon))
            toolbar.FontSize = first.Font.Size;

        if (inlines.All(x => x.LineHeight == first.LineHeight))
            toolbar.Spacing = first.LineHeight;

        if (inlines.All(x => x.Font.Bold == first.Font.Bold))
            toolbar.Bold = first.Font.Bold;

        if (inlines.All(x => x.Font.Italic == first.Font.Italic))
            toolbar.Italic = first.Font.Italic;

        if (inlines.All(x => x.Fill == first.Fill))
            toolbar.Fill = first.Fill;

        if (inlines.All(x => Equals(x.FillPaintable, first.FillPaintable)))
            toolbar.FillBrush = first.FillPaintable.ToBrush();

        if (inlines.All(x => Equals(x.StrokePaintable, first.StrokePaintable)))
            toolbar.StrokeBrush = first.StrokePaintable.ToBrush();

        if (inlines.All(x => Math.Abs(x.StrokeWidth - first.StrokeWidth) < float.Epsilon))
            toolbar.ToolSize = first.StrokeWidth;
    }

    private IReadOnlyList<TextInline> GetEditingInlines(RichText text)
    {
        int cursor = document.TextOverlayHandler.CursorPosition;
        int selectionEnd = document.TextOverlayHandler.SelectionEnd;

        if (cursor == selectionEnd)
        {
            return text.Inlines.ToArray();
        }

        int selectionStart = Math.Min(cursor, selectionEnd);
        int selectionFinish = Math.Max(cursor, selectionEnd);

        List<TextInline> result = new();

        int position = 0;

        foreach (TextInline inline in text.Inlines)
        {
            int inlineStart = position;
            int inlineEnd = position + inline.Text.Length;

            if (inlineStart < selectionFinish && inlineEnd > selectionStart)
            {
                result.Add(inline);
            }

            position = inlineEnd;
        }

        return result;
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
        if (!document.TextOverlayHandler.IsActive)
            return;

        if (isListeningForValidLayer)
            return;

        var text = lastText.Clone();

        int cursor = document.TextOverlayHandler.CursorPosition;
        int selectionEnd = document.TextOverlayHandler.SelectionEnd;

        if (cursor == selectionEnd)
        {
            ApplySettingToAllInlines(text, name, value);
        }
        else
        {
            ApplySettingToSelection(text, name, value, cursor, selectionEnd);
        }

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

        internals.ActionAccumulator.AddActions(
            new SetShapeGeometry_Action(
                selectedMember.Id,
                constructedText,
                changeType),
            new SetLowDpiRendering_Action(
                selectedMember.Id,
                toolbar.ForceLowDpiRendering));

        document.TextOverlayHandler.Text = text;
        lastText = text;
    }

    private void ApplySettingToAllInlines(RichText text, string name, object value)
    {
        for (int i = 0; i < text.Inlines.Count; i++)
        {
            TextInline inline = text.Inlines[i];

            ApplySetting(inline, name, value);

            text.UpdateInline(i, inline);
        }

        MergeAllAdjacentInlines(text);
    }

    private void ApplySettingToSelection(
        RichText text,
        string name,
        object value,
        int cursor,
        int selectionEnd)
    {
        int selectionStart = Math.Min(cursor, selectionEnd);
        int selectionFinish = Math.Max(cursor, selectionEnd);

        int position = 0;

        for (int i = 0; i < text.Inlines.Count; i++)
        {
            TextInline inline = text.Inlines[i];

            int inlineStart = position;
            int inlineEnd = position + inline.Text.Length;

            if (inlineStart < selectionFinish && inlineEnd > selectionStart)
            {
                int localStart = Math.Max(selectionStart, inlineStart) - inlineStart;
                int localEnd = Math.Min(selectionFinish, inlineEnd) - inlineStart;

                if (localStart == 0 && localEnd == inline.Text.Length)
                {
                    ApplySetting(inline, name, value);
                    text.UpdateInline(i, inline);
                }
                else
                {
                    TextInline selectedInline = text.SplitInline(
                        i,
                        inlineStart + localStart,
                        inlineStart + localEnd);

                    ApplySetting(selectedInline, name, value);

                    int selectedIndex = text.IndexOfInline(selectedInline);
                    text.UpdateInline(selectedIndex, selectedInline);
                }
            }

            position = inlineEnd;
        }

        MergeAllAdjacentInlines(text);
    }

    private static void ApplySetting(TextInline inline, string name, object value)
    {
        if (name == nameof(ITextToolbar.FontFamily))
        {
            inline.Font = inline.Font with
            {
                Family = (FontFamilyName)value
            };
        }
        else if (name == nameof(ITextToolbar.FontSize))
        {
            inline.Font = inline.Font with
            {
                Size = (double)value
            };
        }
        else if (name == nameof(ITextToolbar.Fill))
        {
            inline.Fill = (bool)value;
        }
        else if (name == nameof(ITextToolbar.FillBrush))
        {
            inline.FillPaintable = ((IBrush)value).ToPaintable();
        }
        else if (name == nameof(ITextToolbar.ToolSize))
        {
            inline.StrokeWidth = (float)(double)value;
        }
        else if (name == nameof(ITextToolbar.StrokeBrush))
        {
            inline.StrokePaintable = ((IBrush)value).ToPaintable();
        }
        else if (name == nameof(ITextToolbar.Spacing))
        {
            inline.LineHeight = (float)(double)value;
        }
        else if (name == nameof(ITextToolbar.Bold))
        {
            inline.Font = inline.Font with
            {
                Bold = (bool)value
            };
        }
        else if (name == nameof(ITextToolbar.Italic))
        {
            inline.Font = inline.Font with
            {
                Italic = (bool)value
            };
        }
    }

    private static void MergeAllAdjacentInlines(RichText text)
    {
        for (int i = 0; i < text.Inlines.Count - 1;)
        {
            TextInline first = text.Inlines[i];
            TextInline second = text.Inlines[i + 1];

            if (first.HasEqualSettings(second))
            {
                text.MergeAdjacentInlines(first);
            }
            else
            {
                i++;
            }
        }
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
