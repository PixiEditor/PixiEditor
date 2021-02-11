using System;
using System.Linq;
using PixiEditor.Models.DataHolders;
using PixiEditor.ViewModels;

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
    }
}