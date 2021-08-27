using PixiEditor.Models.DataHolders;
using PixiEditor.Models.ImageManipulation;
using PixiEditor.ViewModels;
using System;
using System.Linq;
using System.Windows;

namespace PixiEditor.Models.Layers
{
    public static class LayerHelper
    {
        public static Layer FindLayerByGuid(Document document, Guid guid)
        {
            return document.Layers.FirstOrDefault(x => x.LayerGuid == guid);
        }

        public static object FindLayerByGuidProcess(object[] parameters)
        {
            if (parameters != null && parameters.Length > 0 && parameters[0] is Guid guid)
            {
                return FindLayerByGuid(ViewModelMain.Current.BitmapManager.ActiveDocument, guid);
            }

            return null;
        }

        /// <summary>
        /// Gets the closer layers to the axises.
        /// </summary>
        /// <param name="xCloser">The layer closer to the x Axis.</param>
        /// <param name="yCloser">The layer closer to the y Axis.</param>
        /// <param name="xOther">The other layer that is not closer to the x axis.</param>
        /// <param name="yOther">The other layer that is not closer to the y axis.</param>
        public static void GetCloser(this Layer layer1, Layer layer2, out Layer xCloser, out Layer yCloser, out Layer xOther, out Layer yOther)
        {
            if (layer2.OffsetX > layer1.OffsetX)
            {
                xCloser = layer1;
                xOther = layer2;
            }
            else
            {
                xCloser = layer2;
                xOther = layer1;
            }

            if (layer2.OffsetY > layer1.OffsetY)
            {
                yCloser = layer1;
                yOther = layer2;
            }
            else
            {
                yCloser = layer2;
                yOther = layer1;
            }
        }

        public static Layer MergeWith(this Layer thisLayer, Layer otherLayer, string newName, Vector documentsSize)
        {
            thisLayer.GetCloser(otherLayer, out Layer xCloser, out Layer yCloser, out Layer xOther, out Layer yOther);

            // Calculate the offset to the other layer
            int offsetX = Math.Abs(xCloser.OffsetX + xCloser.Width - xOther.OffsetX);
            int offsetY = Math.Abs(yCloser.OffsetY + yCloser.Height - yOther.OffsetY);

            // Calculate the needed width and height of the new layer
            int width = xCloser.Width + offsetX + xOther.Width;
            int height = yCloser.Height + offsetY + yOther.Height;

            // Merge both layers into a bitmap
            Surface mergedBitmap = BitmapUtils.CombineLayers((int)documentsSize.X, (int)documentsSize.Y, new Layer[] { thisLayer, otherLayer });
            mergedBitmap = mergedBitmap.Crop(xCloser.OffsetX, yCloser.OffsetY, width, height);

            // Create the new layer with the merged bitmap
            Layer mergedLayer = new Layer(newName, mergedBitmap)
            {
                Offset = new Thickness(xCloser.OffsetX, yCloser.OffsetY, 0, 0)
            };

            return mergedLayer;
        }

        public static Layer MergeWith(this Layer thisLayer, Layer otherLayer, string newName, int documentWidth, int documentHeight)
        {
            return MergeWith(thisLayer, otherLayer, newName, new Vector(documentWidth, documentHeight));
        }
    }
}
