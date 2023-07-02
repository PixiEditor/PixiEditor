using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace PixiEditor.Helpers;

internal static class VersionHelpers
{
    public static Version GetCurrentAssemblyVersion() => Assembly.GetExecutingAssembly().GetName().Version;

    public static string GetCurrentAssemblyVersion(Func<Version, string> toString) => toString(GetCurrentAssemblyVersion());

    public static string GetCurrentAssemblyVersionString(bool moreSpecific = false)
    {
        StringBuilder builder = new(GetCurrentAssemblyVersion().ToString());

#if DEVRELEASE
        builder.Append(" Dev Build");
        return builder.ToString();
#elif DEVSTEAM
        builder.Append(" Dev Steam Build");
        return builder.ToString();
#elif MSIX_DEBUG
        builder.Append(" MSIX Debug Build");
        return builder.ToString();
#elif DEBUG
        builder.Append(" Debug Build");
        return builder.ToString();
#endif

        if (!moreSpecific)
            return builder.ToString();

#if STEAM
        builder.Append(" Steam Build");
#elif MSIX
        builder.Append(" MSIX Build");
#elif RELEASE
        builder.Append(" Release Build");
#endif
        return builder.ToString();
    }
}
