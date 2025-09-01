namespace PixiEditor.ViewModels.Nodes;

public enum Traverse
{
    /// <summary>
    /// Go further in this direction, meaning any further child connections will not be enqueued.
    /// </summary>
    Further,
    
    /// <summary>
    /// Don't go further in this direction, meaning all further child connections will be enqueued.
    /// </summary>
    NoFurther,
    
    /// <summary>
    /// Completely stop traversing in any direction, meaning this will drop all enqueued child connections.
    /// </summary>
    Exit
}
