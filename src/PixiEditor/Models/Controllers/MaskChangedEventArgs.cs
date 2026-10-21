namespace PixiEditor.Models.Controllers;

internal class MaskChangedEventArgs : EventArgs
{
    public Guid StructureMemberId { get; }
    public bool HasMask { get; }

    public MaskChangedEventArgs(Guid structureMemberId, bool hasMask)
    {
        StructureMemberId = structureMemberId;
        HasMask = hasMask;
    }
}
