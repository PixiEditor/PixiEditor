using Avalonia.Input;
using Avalonia.Threading;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Text;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.DocumentModels.UpdateableChangeExecutors.Features;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

internal class VectorTextToolExecutor : UpdateableChangeExecutor, ITextOverlayEvents, IQuickToolSwitchable
{
    private ITextToolHandler textHandler;
    private ITextToolbar toolbar;
    private IStructureMemberHandler selectedMember;

    private string lastText = "";
    private VecD position;
    private Matrix3X3 lastMatrix = Matrix3X3.Identity;
    private Font? cachedFont;

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
            return ExecutionState.Error;
        }

        var shape = layerHandler.GetShapeData(document.AnimationHandler.ActiveFrameBindable);
        if (shape is TextVectorData textData)
        {
            document.TextOverlayHandler.Show(textData.Text, textData.Position, textData.Font,
                textData.TransformationMatrix, textData.Spacing);
            lastText = textData.Text;
            position = textData.Position;
            lastMatrix = textData.TransformationMatrix;
        }
        else if (shape is null)
        {
            document.TextOverlayHandler.Show("", controller.LastPrecisePosition, toolbar.ConstructFont(),
                Matrix3X3.Identity, toolbar.Spacing);
            lastText = "";
            position = controller.LastPrecisePosition;
        }
        else
        {
            return ExecutionState.Error;
        }

        return ExecutionState.Success;
    }


    public void OnQuickToolSwitch()
    {
        document.TextOverlayHandler.SetCursorPosition(internals.ChangeController.LastPrecisePosition);
    }

    public override void ForceStop()
    {
        internals.ActionAccumulator.AddFinishedActions(new EndSetShapeGeometry_Action());
        document.TextOverlayHandler.Hide();
    }

    public void OnTextChanged(string text)
    {
        var constructedText = ConstructTextData(text);
        internals.ActionAccumulator.AddFinishedActions(
            new SetShapeGeometry_Action(selectedMember.Id, constructedText),
            new EndSetShapeGeometry_Action(),
            new SetLowDpiRendering_Action(selectedMember.Id, toolbar.ForceLowDpiRendering));
        lastText = text;
        document.TextOverlayHandler.Font = constructedText.Font;
    }

    public override void OnSettingsChanged(string name, object value)
    {
        if (name == nameof(ITextToolbar.FontFamily))
        {
            Font toDispose = cachedFont;
            Dispatcher.UIThread.Post(() =>
            {
                toDispose?.Dispose();
            });

            cachedFont = toolbar.ConstructFont();
            document.TextOverlayHandler.Font = cachedFont;
        }
        else if (name is nameof(ITextToolbar.FontSize))
        {
            if (cachedFont == null)
            {
                cachedFont = toolbar.ConstructFont();
            }

            document.TextOverlayHandler.Font.Size = toolbar.FontSize;
            cachedFont.Size = toolbar.FontSize;
        }

        var constructedText = ConstructTextData(lastText);
        internals.ActionAccumulator.AddActions(
            new SetShapeGeometry_Action(selectedMember.Id, constructedText),
            new SetLowDpiRendering_Action(selectedMember.Id, toolbar.ForceLowDpiRendering));

        document.TextOverlayHandler.Font = constructedText.Font;
        document.TextOverlayHandler.Spacing = toolbar.Spacing;
    }

    public override void OnColorChanged(Color color, bool primary)
    {
        if (!primary || !toolbar.SyncWithPrimaryColor)
        {
            return;
        }

        toolbar.StrokeColor = color.ToColor();
        toolbar.FillColor = color.ToColor();
    }

    private TextVectorData ConstructTextData(string text)
    {
        if (cachedFont == null || cachedFont.Family.Name != toolbar.FontFamily.Name)
        {
            Font toDispose = cachedFont;
            Dispatcher.UIThread.Post(() =>
            {
                toDispose?.Dispose();
            });
            cachedFont = toolbar.ConstructFont();
        }
        else
        {
            cachedFont.Size = toolbar.FontSize;
        }

        return new TextVectorData()
        {
            Text = text,
            Position = position,
            Fill = toolbar.Fill,
            FillColor = toolbar.FillColor.ToColor(),
            StrokeWidth = (float)toolbar.ToolSize,
            StrokeColor = toolbar.StrokeColor.ToColor(),
            TransformationMatrix = lastMatrix,
            Font = cachedFont,
            Spacing = toolbar.Spacing,
            AntiAlias = toolbar.AntiAliasing,
            // TODO: MaxWidth = toolbar.MaxWidth
            // TODO: Path
        };
    }

    bool IExecutorFeature.IsFeatureEnabled(IExecutorFeature feature)
    {
        return feature is ITextOverlayEvents || feature is IQuickToolSwitchable;
    }
}
