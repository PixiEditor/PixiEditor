namespace PixiEditor.Models.DataHolders;

internal class DocumentSizeChangedEventArgs
{
    public DocumentSizeChangedEventArgs(int oldWidth, int oldHeight, int newWidth, int newHeight)
    {
        OldWidth = oldWidth;
        OldHeight = oldHeight;
        NewWidth = newWidth;
        NewHeight = newHeight;
    }

    public int OldWidth { get; set; }

    public int OldHeight { get; set; }

    public int NewWidth { get; set; }

    public int NewHeight { get; set; }
}
