using System.Windows;

namespace PixiEditor.Helpers;

class ClipboardHelper
{
    public static bool TrySetDataObject(DataObject obj, bool copy)
    {
        try
        {
            Clipboard.SetDataObject(obj, copy);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static DataObject TryGetDataObject()
    {
        try
        {
            return (DataObject)Clipboard.GetDataObject();
        }
        catch
        {
            return null;
        }
    }

    public static bool TryClear()
    {
        try
        {
            Clipboard.Clear();
            return true;
        }
        catch
        {
            return false;
        }
    }
}