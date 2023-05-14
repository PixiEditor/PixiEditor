using System.Diagnostics;
using System.Security.AccessControl;
using System.Windows;
using Microsoft.Win32;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Localization;

namespace PixiEditor.Helpers;

public static class RegistryHelpers
{
    public static bool IsKeyPresentInRoot(string keyName)
    {
        using var key = Registry.ClassesRoot.OpenSubKey(keyName, RegistryRights.ReadKey);
        return key != null;
    }

    public static bool TryAssociate(Action associationMethod, LocalizedString errorMessage)
    {
        try
        {
            if (!ProcessHelper.IsRunningAsAdministrator())
            {
                ProcessHelper.RunAsAdmin(Process.GetCurrentProcess().MainModule?.FileName);
                Application.Current.Shutdown();
            }
            else
            {
                associationMethod();
            }
        }
        catch
        {
            NoticeDialog.Show(errorMessage, "ERROR");
        }

        return false;
    }
}
