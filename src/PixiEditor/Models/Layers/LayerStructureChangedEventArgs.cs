using System;
using System.Collections.Generic;

namespace PixiEditor.Models.Layers;

public class LayerStructureChangedEventArgs : EventArgs
{
    public List<Guid> AffectedLayerGuids { get; set; }

    public LayerStructureChangedEventArgs(List<Guid> affectedLayerGuids)
    {
        AffectedLayerGuids = affectedLayerGuids;
    }

    public LayerStructureChangedEventArgs(Guid affectedLayerGuid)
    {
        AffectedLayerGuids = new List<Guid>() { affectedLayerGuid };
    }
}