namespace PixiEditor.DrawingApi.Core.Surfaces;

/// <summary>Possible values to interpret the incoming array of points.</summary>
public enum PointMode
{
    /// <summary>Interpret the data as coordinates for points.</summary>
    Points,
    
    /// <summary>Interpret the data as coordinates for lines.</summary>
    Lines,
    
    /// <summary>Interpret the data as coordinates for polygons.</summary>
    Polygon,
}
