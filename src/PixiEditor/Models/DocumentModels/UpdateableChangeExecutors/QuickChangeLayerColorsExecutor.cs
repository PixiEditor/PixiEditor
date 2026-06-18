using Drawie.Backend.Core.ColorsImpl;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.Models.DocumentModels.UpdateableChangeExecutors.Features;
using PixiEditor.Models.Tools;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

internal class QuickChangeLayerColorsExecutor : UpdateableChangeExecutor, IQuickColorLayerExecutor
{
    public Guid[] LayerGuids { get; }
    public Color Color { get; }

    public override bool BlocksOtherActions => false;

    public QuickChangeLayerColorsExecutor(Guid[] layerGuids, Color color)
    {
        LayerGuids = layerGuids;
        Color = color;
    }

    public override ExecutionState Start()
    {
        if (document == null || internals == null || controller == null)
            return ExecutionState.Error;

        EnqueueColorChange(Color);

        return ExecutionState.Success;
    }

    private void EnqueueColorChange(Color color)
    {
       internals.ActionAccumulator.AddActions(new QuickChangeLayersColor_Action(LayerGuids, color));
    }

    public override void OnColorChanged(Color color, bool primary)
    {
        if (primary)
        {
            EnqueueColorChange(color);
        }
    }

    public override void ForceStop()
    {
        Stop();
    }

    private void Stop()
    {
        internals.ActionAccumulator.AddFinishedActions(new EndQuickChangeLayersColor_Action());
    }

    public bool IsFeatureEnabled<T>()
    {
        return typeof(T) == typeof(IQuickColorLayerExecutor);
    }

    public void EndQuickColorChange()
    {
        internals.ChangeController.TryStopActiveExecutor();
    }
}
