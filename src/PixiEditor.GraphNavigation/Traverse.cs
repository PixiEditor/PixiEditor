namespace PixiEditor.GraphNavigation;

/// <summary>
/// Specifies how a graph or tree traversal should proceed after evaluating a node.
/// </summary>
public enum Traverse
{
    /// <summary>
    /// Continues traversal into the current node's child connections.
    /// </summary>
    Continue,

    /// <summary>
    /// Skips the current node's child connections without halting overall traversal.
    /// </summary>
    SkipChildren,

    /// <summary>
    /// Halts traversal immediately and includes the current node in the final result.
    /// </summary>
    ExitInclusive,

    /// <summary>
    /// Halts traversal immediately and excludes the current node from the final result.
    /// </summary>
    ExitExclusive
}
