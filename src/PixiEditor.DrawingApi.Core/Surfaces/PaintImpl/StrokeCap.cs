namespace PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;

/// <summary>Various options for <see cref="Paint.StrokeCap" />.</summary>
/// <remarks>This is the treatment that is applied to the beginning and end of each non-closed contour (e.g. lines).</remarks>
public enum StrokeCap
{
    /// <summary>Begin/end contours with no extension.</summary>
    Butt,
    /// <summary>Begin/end contours with a semi-circle extension.</summary>
    Round,
    /// <summary>Begin/end contours with a half square extension.</summary>
    Square,
}
