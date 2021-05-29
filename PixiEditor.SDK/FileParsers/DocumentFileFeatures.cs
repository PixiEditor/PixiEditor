using System;

namespace PixiEditor.SDK.FileParsers
{
    [Flags]
    public enum DocumentFileFeatures
    {
        /// <summary>
        /// Supports all features that a .pixi file supports
        /// </summary>
        All = Basic | Layers | Swatches,

        /// <summary>
        /// Supports saving image, store document size, ... <para>This is required</para>
        /// </summary>
        Basic = 0,
        Layers = 1,
        Swatches = 2
    }
}
