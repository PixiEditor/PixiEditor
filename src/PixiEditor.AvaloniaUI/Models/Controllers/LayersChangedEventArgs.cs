using PixiEditor.AvaloniaUI.Models.Layers;

namespace PixiEditor.AvaloniaUI.Models.Controllers;

internal class LayersChangedEventArgs : EventArgs
{
    public LayersChangedEventArgs(Guid layerAffectedGuid, LayerAction layerChangeType)
    {
        LayerAffectedGuid = layerAffectedGuid;
        LayerChangeType = layerChangeType;
    }

    public Guid LayerAffectedGuid { get; set; }

    public LayerAction LayerChangeType { get; set; }
}
