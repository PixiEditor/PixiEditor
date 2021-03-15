using System.Collections.Generic;

namespace PixiEditor.Models.Layers
{
    public interface ILayerContainer
    {
        public IEnumerable<Layer> GetLayers();
    }
}