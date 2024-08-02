namespace PixiEditor.DrawingApi.Core.Surfaces;

public enum TileMode
{
    /// <summary>Replicate the edge color.</summary>
    Clamp,
    
    /// <summary>Repeat the shader's image horizontally and vertically.</summary>
    Repeat,
    
    /// <summary>Repeat the shader's image horizontally and vertically, alternating mirror images so that adjacent images always seam.</summary>
    Mirror,
    
    /// <summary>To be added.</summary>
    Decal,
}
