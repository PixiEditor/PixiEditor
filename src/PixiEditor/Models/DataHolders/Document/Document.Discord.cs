namespace PixiEditor.Models.DataHolders.Document;

internal partial class Document
{
    private readonly DateTime openedUtc = DateTime.UtcNow;

    public DateTime OpenedUTC
    {
        get => openedUtc;
    }
}
