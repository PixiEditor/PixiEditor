using System;
using PixiEditor.Models.Enums;

namespace PixiEditor.Models.Controllers
{
    public class LayersChangedEventArgs : EventArgs
    {
        public LayersChangedEventArgs(Guid layerAffectedGuid, LayerAction layerChangeType)
        {
            LayerAffectedGuid = layerAffectedGuid;
            LayerChangeType = layerChangeType;
        }

        public Guid LayerAffectedGuid { get; set; }

        public LayerAction LayerChangeType { get; set; }
    }
}