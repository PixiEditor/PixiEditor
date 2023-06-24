using System.Windows;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.Helpers;

internal static class ClipboardHelper
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
    
    public static VecI GetVecI(this DataObject data, string format) => VecI.FromBytes((byte[])data.GetData(format));

    public static void SetVecI(this DataObject data, string format, VecI value) => data.SetData(format, value.ToByteArray());
}
