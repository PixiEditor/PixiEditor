using PixiEditor.Models.DataHolders;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PixiEditor.Models.Controllers
{
    public class PixelChangesController
    {
        private Dictionary<Guid, LayerChange> LastChanges { get; set; }

        private Dictionary<Guid, LayerChange> LastOldValues { get; set; }

        /// <summary>
        ///     Adds layer changes to controller.
        /// </summary>
        /// <param name="changes">New changes.</param>
        /// <param name="oldValues">Old values of changes.</param>
        public void AddChanges(LayerChange changes, LayerChange oldValues)
        {
            if (changes.PixelChanges.ChangedPixels.Count > 0)
            {
                if (LastChanges == null)
                {
                    LastChanges = new Dictionary<Guid, LayerChange> { { changes.LayerGuid, changes } };
                    LastOldValues = new Dictionary<Guid, LayerChange> { { oldValues.LayerGuid, oldValues } };
                }
                else if (LastChanges.ContainsKey(changes.LayerGuid))
                {
                    AddToExistingLayerChange(changes, oldValues);
                }
                else
                {
                    AddNewLayerChange(changes, oldValues);
                }
            }
        }

        /// <summary>
        ///     Returns all changes and deletes them from controller.
        /// </summary>
        /// <returns>Tuple array with new changes and old values.</returns>
        public Tuple<LayerChange, LayerChange>[] PopChanges()
        {
            // Maybe replace Tuple with custom data type
            if (LastChanges == null)
            {
                return null;
            }

            Tuple<LayerChange, LayerChange>[] result = new Tuple<LayerChange, LayerChange>[LastChanges.Count];
            int i = 0;
            foreach (KeyValuePair<Guid, LayerChange> change in LastChanges)
            {
                Dictionary<Position.Coordinates, SKColor> pixelChanges =
                    change.Value.PixelChanges.ChangedPixels.ToDictionary(entry => entry.Key, entry => entry.Value);
                Dictionary<Position.Coordinates, SKColor> oldValues = LastOldValues[change.Key].PixelChanges.ChangedPixels
                    .ToDictionary(entry => entry.Key, entry => entry.Value);

                LayerChange tmp = new LayerChange(new BitmapPixelChanges(pixelChanges), change.Key);
                LayerChange oldValuesTmp = new LayerChange(new BitmapPixelChanges(oldValues), change.Key);

                result[i] = new Tuple<LayerChange, LayerChange>(tmp, oldValuesTmp);
                i++;
            }

            LastChanges = null;
            LastOldValues = null;
            return result;
        }

        private void AddNewLayerChange(LayerChange changes, LayerChange oldValues)
        {
            LastChanges[changes.LayerGuid] = changes;
            LastOldValues[changes.LayerGuid] = oldValues;
        }

        private void AddToExistingLayerChange(LayerChange layerChange, LayerChange oldValues)
        {
            foreach (KeyValuePair<Position.Coordinates, SKColor> change in layerChange.PixelChanges.ChangedPixels)
            {
                if (LastChanges[layerChange.LayerGuid].PixelChanges.ChangedPixels.ContainsKey(change.Key))
                {
                    continue;
                }
                else
                {
                    LastChanges[layerChange.LayerGuid].PixelChanges.ChangedPixels.Add(change.Key, change.Value);
                }
            }

            foreach (KeyValuePair<Position.Coordinates, SKColor> change in oldValues.PixelChanges.ChangedPixels)
            {
                if (LastOldValues[layerChange.LayerGuid].PixelChanges.ChangedPixels.ContainsKey(change.Key))
                {
                    continue;
                }
                else
                {
                    LastOldValues[layerChange.LayerGuid].PixelChanges.ChangedPixels.Add(change.Key, change.Value);
                }
            }
        }
    }
}
