using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("KeyboardInfo")]
public class KeyboardInfoNode : Node
{
    public OutputProperty<bool> IsCtrlPressed { get; }
    public OutputProperty<bool> IsShiftPressed { get; }
    public OutputProperty<bool> IsAltPressed { get; }
    public OutputProperty<bool> IsMetaPressed { get; }

    public KeyboardInfoNode()
    {
        IsCtrlPressed = CreateOutput<bool>("IsCtrlPressed", "IS_CTRL_PRESSED", false);
        IsShiftPressed = CreateOutput<bool>("IsShiftPressed", "IS_SHIFT_PRESSED", false);
        IsAltPressed = CreateOutput<bool>("IsAltPressed", "IS_ALT_PRESSED", false);
        IsMetaPressed = CreateOutput<bool>("IsMetaPressed", "IS_META_PRESSED", false);
    }

    protected override void OnExecute(RenderContext context)
    {
        IsCtrlPressed.Value = context.KeyboardInfo.IsCtrlPressed;
        IsShiftPressed.Value = context.KeyboardInfo.IsShiftPressed;
        IsAltPressed.Value = context.KeyboardInfo.IsAltPressed;
        IsMetaPressed.Value = context.KeyboardInfo.IsMetaPressed;
    }

    public override Node CreateCopy()
    {
        return new KeyboardInfoNode();
    }
}
