using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace PixiEditor.Models.Controllers
{
    public class PixelChangesController
    {
        LayerChanges LastChanges { get; set; }
        LayerChanges LastOldValues { get; set; }

        public void AddChanges(LayerChanges changes, LayerChanges oldValues)
        {
            if(LastChanges == null)
            {
                LastChanges = changes;
                LastOldValues = oldValues;
                return;
            }

            foreach (var change in changes.PixelChanges.ChangedPixels)
            {
                if (LastChanges.PixelChanges.ChangedPixels.ContainsKey(change.Key))
                {
                    continue;
                }
                else
                {
                    LastChanges.PixelChanges.ChangedPixels.Add(change.Key, change.Value);
                }
            }

            foreach (var change in oldValues.PixelChanges.ChangedPixels)
            {
                if (LastOldValues.PixelChanges.ChangedPixels.ContainsKey(change.Key))
                {
                    continue;
                }
                else
                {
                    LastOldValues.PixelChanges.ChangedPixels.Add(change.Key, change.Value);
                }
            }
        }

        public Tuple<LayerChanges, LayerChanges> PopChanges()
        {
            Dictionary<Coordinates, Color> pixelChanges = LastChanges.PixelChanges.ChangedPixels.ToDictionary(entry => entry.Key, entry => entry.Value);
            Dictionary<Coordinates, Color> oldValues = LastOldValues.PixelChanges.ChangedPixels.ToDictionary(entry => entry.Key, entry => entry.Value);
            
            var tmp = new LayerChanges(new BitmapPixelChanges(pixelChanges), LastChanges.LayerIndex);
            var oldValuesTmp = new LayerChanges(new BitmapPixelChanges(oldValues), LastOldValues.LayerIndex);
            
            Tuple<LayerChanges, LayerChanges> outputChanges = new Tuple<LayerChanges, LayerChanges>(tmp, oldValuesTmp);
            LastChanges.PixelChanges.ChangedPixels.Clear();
            LastOldValues.PixelChanges.ChangedPixels.Clear();
            return outputChanges;
        }
    }
}
