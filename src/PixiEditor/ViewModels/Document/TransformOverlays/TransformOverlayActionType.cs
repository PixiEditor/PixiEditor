namespace PixiEditor.ViewModels.Document.TransformOverlays;
internal enum TransformOverlayStateType
{
    /// <summary>
    /// The overlay was moved via mouse
    /// </summary>
    Move,

    /// <summary>
    /// The overlay was nudged using arrows keys
    /// </summary>
    Nudge,

    /// <summary>
    /// The overlay was set to this state when it was enabled
    /// </summary>
    Initial
}

internal static class TransformOverlayStateTypeEx
{
    public static bool IsMergeable(this TransformOverlayStateType type) => type switch
    {
        TransformOverlayStateType.Move => false,
        TransformOverlayStateType.Nudge => true,
        TransformOverlayStateType.Initial => false
    };
}
