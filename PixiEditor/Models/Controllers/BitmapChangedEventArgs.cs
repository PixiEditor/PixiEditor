using PixiEditor.Models.DataHolders;
using System;

namespace PixiEditor.Models.Controllers
{
    public class BitmapChangedEventArgs : EventArgs
    {
        public BitmapChangedEventArgs(BitmapPixelChanges pixelsChanged, BitmapPixelChanges oldPixelsValues, Guid changedLayerGuid)
        {
            PixelsChanged = pixelsChanged;
            OldPixelsValues = oldPixelsValues;
            ChangedLayerGuid = changedLayerGuid;
        }

        public BitmapPixelChanges PixelsChanged { get; set; }

        public BitmapPixelChanges OldPixelsValues { get; set; }

        public Guid ChangedLayerGuid { get; set; }
    }
}