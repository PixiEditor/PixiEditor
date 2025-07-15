using System.Runtime.CompilerServices;

namespace PixiEditor.Extensions.Sdk.Bridge;

internal static partial class Native
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern bool is_user_logged_in();

    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern string get_username();

    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern string get_account_provider_name();

    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern IntPtr get_owned_content();

    [ApiExport("on_user_logged_in")]
    internal static void OnUserLoggedIn()
    {
        PixiEditorExtension.Api.UserDataProvider.OnUserLoggedIn();
    }

    [ApiExport("on_user_logged_out")]
    internal static void OnUserLoggedOut()
    {
        PixiEditorExtension.Api.UserDataProvider.OnUserLoggedOut();
    }
}
