using System.Threading.Tasks;
using System.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Input.Platform;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.Helpers;

internal static class ClipboardHelper
{
    public static async Task<bool> TrySetDataObject(DataObject obj)
    {
        try
        {
            if(Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                 await desktop.MainWindow.Clipboard.SetDataObjectAsync(obj);
                 return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<object> TryGetDataObject(string format)
    {
        try
        {
            if(Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop){
                return await desktop.MainWindow.Clipboard.GetDataAsync(format);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public static async Task<bool> TryClear()
    {
        try
        {
            if(Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                await desktop.MainWindow.Clipboard.ClearAsync();
                return true;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public static VecI GetVecI(this DataObject data, string format)
    {
        if (!data.Contains(format))
            return VecI.NegativeOne;

        byte[] bytes = (byte[])data.Get(format);

        if (bytes is { Length: < 8 })
            return VecI.NegativeOne;

        return VecI.FromBytes(bytes);
    }

    public static void SetVecI(this DataObject data, string format, VecI value) => data.Set(format, value.ToByteArray());
}
