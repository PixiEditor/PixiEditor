using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.Models.Layers.Utils
{
    public static class LayerStructureUtils
    {
        /// <summary>
        /// Gets final layer opacity taking into consideration parent groups.
        /// </summary>
        /// <param name="layer">Layer to check.</param>
        /// <returns>Float from range 0-1.</returns>
        public static float GetFinalLayerOpacity(Layer layer, LayerStructure structure)
        {
            if (layer.Opacity == 0)
            {
                return 0f;
            }

            var group = structure.GetGroupByLayer(layer.LayerGuid);
            GuidStructureItem groupToCheck = group;
            float finalOpacity = layer.Opacity;

            while (groupToCheck != null)
            {
                finalOpacity *= groupToCheck.Opacity;
                groupToCheck = groupToCheck.Parent;
            }

            return Math.Clamp(finalOpacity, 0f, 1f);
        }
    }
}