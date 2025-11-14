namespace PixiEditor.ChangeableDocument.Rendering.ContextData;

public record struct KeyboardInfo
{
    public bool IsCtrlPressed { get; }
    public bool IsShiftPressed { get; }
    public bool IsAltPressed { get; }
    public bool IsMetaPressed { get; }

    public KeyboardInfo(bool isCtrlPressed, bool isShiftPressed, bool isAltPressed, bool isMetaPressed)
    {
        IsCtrlPressed = isCtrlPressed;
        IsShiftPressed = isShiftPressed;
        IsAltPressed = isAltPressed;
        IsMetaPressed = isMetaPressed;
    }
}
