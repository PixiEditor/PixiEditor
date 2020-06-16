using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;

namespace PixiEditor.Models.Controllers
{
    public class PixelChangesController
    {
        private Dictionary<int, LayerChange> LastChanges { get; set; }
        private Dictionary<int, LayerChange> LastOldValues { get; set; }

        public void AddChanges(LayerChange changes, LayerChange oldValues)
        {
            if (changes.PixelChanges.ChangedPixels.Count > 0)
            {
                if (LastChanges == null)
                {
                    LastChanges = new Dictionary<int, LayerChange> {{changes.LayerIndex, changes}};
                    LastOldValues = new Dictionary<int, LayerChange> {{oldValues.LayerIndex, oldValues}};
                }
                else if (LastChanges.ContainsKey(changes.LayerIndex))
                {
                    AddToExistingLayerChange(changes, oldValues);
                }
                else
                {
                    AddNewLayerChange(changes, oldValues);
                }
            }
        }

        private void AddNewLayerChange(LayerChange changes, LayerChange oldValues)
        {
            LastChanges[changes.LayerIndex] = changes;
            LastOldValues[changes.LayerIndex] = oldValues;
        }

        private void AddToExistingLayerChange(LayerChange layerChange, LayerChange oldValues)
        {
            foreach (var change in layerChange.PixelChanges.ChangedPixels)
                if (LastChanges[layerChange.LayerIndex].PixelChanges.ChangedPixels.ContainsKey(change.Key))
                    continue;
                else
                    LastChanges[layerChange.LayerIndex].PixelChanges.ChangedPixels.Add(change.Key, change.Value);

            foreach (var change in oldValues.PixelChanges.ChangedPixels)
                if (LastOldValues[layerChange.LayerIndex].PixelChanges.ChangedPixels.ContainsKey(change.Key))
                    continue;
                else
                    LastOldValues[layerChange.LayerIndex].PixelChanges.ChangedPixels.Add(change.Key, change.Value);
        }

        public Tuple<LayerChange, LayerChange>[] PopChanges()
        {
            //Maybe replace Tuple with custom data type
            if (LastChanges == null) return null;
            Tuple<LayerChange, LayerChange>[] result = new Tuple<LayerChange, LayerChange>[LastChanges.Count];
            int i = 0;
            foreach (var change in LastChanges)
            {
                Dictionary<Coordinates, Color> pixelChanges =
                    change.Value.PixelChanges.ChangedPixels.ToDictionary(entry => entry.Key, entry => entry.Value);
                Dictionary<Coordinates, Color> oldValues = LastOldValues[change.Key].PixelChanges.ChangedPixels
                    .ToDictionary(entry => entry.Key, entry => entry.Value);

                var tmp = new LayerChange(new BitmapPixelChanges(pixelChanges), change.Key);
                var oldValuesTmp = new LayerChange(new BitmapPixelChanges(oldValues), change.Key);

                result[i] = new Tuple<LayerChange, LayerChange>(tmp, oldValuesTmp);
                i++;
            }

            LastChanges = null;
            LastOldValues = null;
            return result;
        }
    }
}