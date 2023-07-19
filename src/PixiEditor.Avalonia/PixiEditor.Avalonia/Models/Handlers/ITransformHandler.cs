namespace PixiEditor.Models.Containers;

public interface ITransformHandler
{
    public void ModifierKeysInlet(bool argsIsShiftDown, bool argsIsCtrlDown, bool argsIsAltDown);
}
