namespace PixiEditor.Numerics;

/// <summary>
/// A helper class for creating 4x5 color matrices
/// </summary>
public static class ColorMatrix
{
    /// <summary>
    /// All values are set to 0. <br/>
    /// (_, _, _, _) => (0, 0, 0, 0)
    /// </summary>
    public static Matrix4x5F Zero => new(
        (0, 0, 0, 0, 0),
        (0, 0, 0, 0, 0),
        (0, 0, 0, 0, 0),
        (0, 0, 0, 0, 0)
    );

    /// <summary>
    /// All values stay the same. <br/>
    /// (x, y, z, w) => (x, y, z, w)
    /// </summary>
    public static Matrix4x5F Identity => new(
        (1, 0, 0, 0, 0),
        (0, 1, 0, 0, 0),
        (0, 0, 1, 0, 0),
        (0, 0, 0, 1, 0)
    );

    /// <summary>
    /// Values are offset by r, g, b and a <br/>
    /// (x, y, z, w) => (x + <paramref name="r"/>, y + <paramref name="g"/>, z + <paramref name="b"/>, w + <paramref name="a"/>)
    /// </summary>
    public static Matrix4x5F Offset(float r, float g, float b, float a) => new(
        (0, 0, 0, 0, r),
        (0, 0, 0, 0, g),
        (0, 0, 0, 0, b),
        (0, 0, 0, 0, a)
    );

    /// <summary>
    /// The Red value is mapped to the Green and Blue values. The Red and Alpha values become 0. <br/><br/>
    /// 
    /// Adding UseRed + UseAlpha will result in grayscale <br/>
    /// (x, _, _, _) => (0, x, x, 0)
    /// </summary>
    public static Matrix4x5F MapRedToGreenBlue => new(
        (0, 0, 0, 0, 0),
        (1, 0, 0, 0, 0),
        (1, 0, 0, 0, 0),
        (0, 0, 0, 0, 0)
    );
    
    /// <summary>
    /// The Green value is mapped to the Red and Blue values. The Green and Alpha values become 0. <br/><br/>
    /// 
    /// Adding UseGreen + UseAlpha will result in grayscale <br/>
    /// (_, y, _, _) => (y, 0, y, 0)
    /// </summary>
    public static Matrix4x5F MapGreenToRedBlue => new(
        (0, 1, 0, 0, 0),
        (0, 0, 0, 0, 0),
        (0, 1, 0, 0, 0),
        (0, 0, 0, 0, 0)
    );

    /// <summary>
    /// The Blue value is mapped to the Red and Green values. The Blue and Alpha values become 0. <br/><br/>
    /// 
    /// Adding UseBlue + UseAlpha will result in grayscale <br/>
    /// (_, _, z, _) => (z, z, 0, 0)
    /// </summary>
    public static Matrix4x5F MapBlueToRedGreen => new(
        (0, 0, 1, 0, 0),
        (0, 0, 1, 0, 0),
        (0, 0, 0, 0, 0),
        (0, 0, 0, 0, 0)
    );

    /// <summary>
    /// The Alpha value is mapped to the Red, Green and Blue values. The Alpha values becomes 0. <br/>
    /// 
    /// (_, _, _, w) => (w, w, w, 0)
    /// </summary>
    public static Matrix4x5F MapAlphaToRedGreenBlue => new(
        (0, 0, 0, 1, 0),
        (0, 0, 0, 1, 0),
        (0, 0, 0, 1, 0),
        (0, 0, 0, 0, 0)
    );

    /// <summary>
    /// The red value will stay the red value <br/>
    /// (x, _, _, _) => (x, 0, 0, 0)
    /// </summary>
    public static Matrix4x5F UseRed => new(
        (1, 0, 0, 0, 0),
        (0, 0, 0, 0, 0),
        (0, 0, 0, 0, 0),
        (0, 0, 0, 0, 0)
    );
    
    /// <summary>
    /// The green value will stay the green value <br/>
    /// (_, y, _, _) => (0, y, 0, 0)
    /// </summary>
    public static Matrix4x5F UseGreen => new(
        (0, 0, 0, 0, 0),
        (0, 1, 0, 0, 0),
        (0, 0, 0, 0, 0),
        (0, 0, 0, 0, 0)
    );
    
    /// <summary>
    /// The blue value will stay the blue value <br/>
    /// (_, _, z, _) => (0, 0, z, 0)
    /// </summary>
    public static Matrix4x5F UseBlue => new(
        (0, 0, 0, 0, 0),
        (0, 0, 0, 0, 0),
        (0, 0, 1, 0, 0),
        (0, 0, 0, 0, 0)
    );
    
    /// <summary>
    /// The alpha value will stay the alpha value <br/>
    /// (_, _, _, w) => (0, 0, 0, w)
    /// </summary>
    public static Matrix4x5F UseAlpha => new(
        (0, 0, 0, 0, 0),
        (0, 0, 0, 0, 0),
        (0, 0, 0, 0, 0),
        (0, 0, 0, 1, 0)
    );

    /// <summary>
    /// The alpha value will be offset by 1 <br/>
    /// (_, _, _, w) => (0, 0, 0, w + 1)
    /// </summary>
    public static Matrix4x5F OpaqueAlphaOffset => Offset(0, 0, 0, 1);
}
