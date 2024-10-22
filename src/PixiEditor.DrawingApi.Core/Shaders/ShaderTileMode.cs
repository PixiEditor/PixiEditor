namespace PixiEditor.DrawingApi.Core.Shaders;

public enum ShaderTileMode
{
    Clamp,

    /// <summary>Repeat the shader's image horizontally and vertically.</summary>
    Repeat,

    /// <summary>Repeat the shader's image horizontally and vertically, alternating mirror images so that adjacent images always seam.</summary>
    Mirror,
    Decal,
}
