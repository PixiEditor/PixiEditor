using SkiaSharp;

namespace PixiEditor.DrawingApi.Core.Surface.ImageData;

/// <summary>Describes how to interpret the components of a pixel.</summary>
public enum ColorType
{
    /// <summary>Unknown encoding.</summary>
    Unknown,
    
    /// <summary>Represents a 8-bit alpha-only color.</summary>
    Alpha8,
    
    /// <summary>Represents an opaque 16-bit color with the format RGB, with the red and blue components being 5 bits and the green component being 6 bits.</summary>
    Rgb565,
    
    /// <summary>Represents a 16-bit color with the format ARGB.</summary>
    Argb4444,
    
    /// <summary>Represents a 32-bit color with the format RGBA.</summary>
    Rgba8888,
    
    /// <summary>Represents an opaque 32-bit color with the format RGB, with 8 bits per color component.</summary>
    Rgb888x,
    
    /// <summary>Represents a 32-bit color with the format BGRA.</summary>
    Bgra8888,
    
    /// <summary>Represents a 32-bit color with the format RGBA, with 10 bits per color component and 2 bits for the alpha component.</summary>
    Rgba1010102,
    
    /// <summary>Represents an opaque 32-bit color with the format RGB, with 10 bits per color component.</summary>
    Rgb101010x,
    
    /// <summary>Represents an opaque 8-bit grayscale color.</summary>
    Gray8,
    
    /// <summary>Represents a floating-point based color with the format RGBA.</summary>
    RgbaF16,
    
    /// <summary>To be added.</summary>
    RgbaF16Clamped,
    
    /// <summary>To be added.</summary>
    RgbaF32,
    
    /// <summary>To be added.</summary>
    Rg88,
    
    /// <summary>To be added.</summary>
    AlphaF16,
    
    /// <summary>To be added.</summary>
    RgF16,
    
    /// <summary>To be added.</summary>
    Alpha16,
    
    /// <summary>To be added.</summary>
    Rg1616,
    
    /// <summary>To be added.</summary>
    Rgba16161616,
}
