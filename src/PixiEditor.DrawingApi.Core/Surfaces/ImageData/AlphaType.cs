namespace PixiEditor.DrawingApi.Core.Surfaces.ImageData;

/// <summary>Describes how to interpret the alpha component of a pixel.</summary>
public enum AlphaType
{
    /// <summary />
    Unknown,
    
    /// <summary>All pixels are stored as opaque.</summary>
    Opaque,
    
    /// <summary>
    ///     <para>All pixels have their alpha premultiplied in their color components.</para>
    ///     <para>This is the natural format for the rendering target pixels.</para>
    /// </summary>
    Premul,
    
    /// <summary>
    ///     <para>All pixels have their color components stored without any regard to the alpha. e.g. this is the default configuration for PNG images.</para>
    ///     <para>This alpha-type is ONLY supported for input images. Rendering cannot generate this on output.</para>
    /// </summary>
    Unpremul,
}
