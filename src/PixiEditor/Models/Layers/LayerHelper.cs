using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.ImageManipulation;
using PixiEditor.ViewModels;
using System;
using System.Linq;
using System.Windows;

namespace PixiEditor.Models.Layers;

public static class LayerHelper
{
    public static Layer FindLayerByGuid(Document document, Guid guid)
    {
        return document.Layers.FirstOrDefault(x => x.GuidValue == guid);
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

    public static Layer MergeWith(this Layer thisLayer, Layer otherLayer, string newName, PixelSize documentSize)
    {
        Int32Rect thisRect = new(thisLayer.OffsetX, thisLayer.OffsetY, thisLayer.Width, thisLayer.Height);
        Int32Rect otherRect = new(otherLayer.OffsetX, otherLayer.OffsetY, otherLayer.Width, otherLayer.Height);

        Int32Rect combined = thisRect.Expand(otherRect);

        Surface mergedBitmap = BitmapUtils.CombineLayers(combined, new Layer[] { thisLayer, otherLayer });

        Layer mergedLayer = new Layer(newName, mergedBitmap, documentSize.Width, documentSize.Height)
        {
            Offset = new Thickness(combined.X, combined.Y, 0, 0),
        };

        return mergedLayer;
    }

    public static Layer MergeWith(this Layer thisLayer, Layer otherLayer, string newName, int documentWidth, int documentHeight)
    {
        return MergeWith(thisLayer, otherLayer, newName, new PixelSize(documentWidth, documentHeight));
    }
}