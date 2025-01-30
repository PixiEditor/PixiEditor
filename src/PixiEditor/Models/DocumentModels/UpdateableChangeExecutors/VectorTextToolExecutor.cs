using Avalonia.Input;
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

internal class VectorTextToolExecutor : UpdateableChangeExecutor, ITextOverlayEvents
{
    private ITextToolHandler textHandler;
    private ITextToolbar toolbar;
    private IStructureMemberHandler selectedMember;

    private string lastText = "";
    private VecD position;
    private Matrix3X3 lastMatrix = Matrix3X3.Identity;

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
            document.TextOverlayHandler.Show(textData.Text, textData.Position, textData.Font, textData.TransformationMatrix, textData.Spacing);
            lastText = textData.Text;
            position = textData.Position;
            lastMatrix = textData.TransformationMatrix;
        }
        else if (shape is null)
        {
            document.TextOverlayHandler.Show("", controller.LastPrecisePosition, toolbar.ConstructFont(), Matrix3X3.Identity, toolbar.Spacing);
            lastText = "";
            position = controller.LastPrecisePosition;
        }
        else
        {
            return ExecutionState.Error;
        }

        return ExecutionState.Success;
    }

    public override void ForceStop()
    {
        internals.ActionAccumulator.AddFinishedActions(new EndSetShapeGeometry_Action());
        document.TextOverlayHandler.Hide();
    }

    public void OnTextChanged(string text)
    {
        var constructedText = ConstructTextData(text);
        internals.ActionAccumulator.AddActions(new SetShapeGeometry_Action(selectedMember.Id, constructedText));
        lastText = text;
        document.TextOverlayHandler.Font = constructedText.Font;
    }

    public override void OnSettingsChanged(string name, object value)
    {
        if (name is nameof(ITextToolbar.FontSize) or nameof(ITextToolbar.FontFamily))
        {
            document.TextOverlayHandler.Font = toolbar.ConstructFont();
        }
        
        var constructedText = ConstructTextData(lastText);
        internals.ActionAccumulator.AddActions(new SetShapeGeometry_Action(selectedMember.Id, constructedText));
        
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
        Font font = toolbar.ConstructFont(); 
        return new TextVectorData()
        {
            Text = text,
            Position = position,
            Fill = toolbar.Fill,
            FillColor = toolbar.FillColor.ToColor(),
            StrokeWidth = (float)toolbar.ToolSize,
            StrokeColor = toolbar.StrokeColor.ToColor(),
            TransformationMatrix = lastMatrix,
            Font = font,
            Spacing = toolbar.Spacing
        };
    }

    bool IExecutorFeature.IsFeatureEnabled(IExecutorFeature feature)
    {
        return feature is ITextOverlayEvents;
    }
}
