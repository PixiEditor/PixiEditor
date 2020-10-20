using System;
using PixiEditor.Models.Enums;

namespace PixiEditor.Models.Controllers
{
    public class LayersChangedEventArgs : EventArgs
    {
        public LayersChangedEventArgs(int layerAffected, LayerAction layerChangeType)
        {
            LayerAffected = layerAffected;
            LayerChangeType = layerChangeType;
        }

        public int LayerAffected { get; set; }

        public LayerAction LayerChangeType { get; set; }
    }
}