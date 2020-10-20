using System;
using PixiEditor.Models.DataHolders;

public class BitmapChangedEventArgs : EventArgs
{
    public BitmapChangedEventArgs(BitmapPixelChanges pixelsChanged, BitmapPixelChanges oldPixelsValues, int changedLayerIndex)
    {
        PixelsChanged = pixelsChanged;
        OldPixelsValues = oldPixelsValues;
        ChangedLayerIndex = changedLayerIndex;
    }

    public BitmapPixelChanges PixelsChanged { get; set; }

    public BitmapPixelChanges OldPixelsValues { get; set; }

    public int ChangedLayerIndex { get; set; }
}