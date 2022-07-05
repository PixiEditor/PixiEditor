namespace PixiEditor.Models.DataHolders;

internal partial class Document
{
    private readonly DateTime openedUtc = DateTime.UtcNow;

    public DateTime OpenedUTC
    {
        get => openedUtc;
    }
}
