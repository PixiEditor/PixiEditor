using System;

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

        /// <summary>
        /// Gets final layer IsVisible taking into consideration group visibility.
        /// </summary>
        /// <param name="layer">Layer to check.</param>
        /// <returns>True if is visible, false if at least parent is not visible or layer itself is invisible.</returns>
        public static bool GetFinalLayerIsVisible(Layer layer, LayerStructure structure)
        {
            if (!layer.IsVisible)
            {
                return false;
            }

            var group = structure.GetGroupByLayer(layer.LayerGuid);
            bool atLeastOneParentIsInvisible = false;
            GuidStructureItem groupToCheck = group;
            while (groupToCheck != null)
            {
                if (!groupToCheck.IsVisible)
                {
                    atLeastOneParentIsInvisible = true;
                    break;
                }

                groupToCheck = groupToCheck.Parent;
            }

            return !atLeastOneParentIsInvisible;
        }
    }
}
