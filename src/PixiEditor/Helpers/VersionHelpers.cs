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

        bool isDone = false;

        AppendDevBuild(builder, ref isDone);
        if (isDone)
            return builder.ToString();

        AppendMsixDebugBuild(builder, ref isDone);
        if (isDone)
            return builder.ToString();

        AppendDebugBuild(builder, ref isDone);
        if (isDone || !moreSpecific)
            return builder.ToString();

        AppendSteamBuild(builder, ref isDone);
        if (isDone)
            return builder.ToString();

        AppendMsixBuild(builder, ref isDone);
        if (isDone)
            return builder.ToString();

        AppendReleaseBuild(builder, ref isDone);
        return builder.ToString();
    }

    [Conditional("DEVRELEASE")]
    private static void AppendDevBuild(StringBuilder builder, ref bool done)
    {
        done = true;

        builder.Append(" Dev Build");
    }

    [Conditional("DEBUG")]
    private static void AppendDebugBuild(StringBuilder builder, ref bool done)
    {
        done = true;

        builder.Append(" Debug Build");
    }

    [Conditional("RELEASE")]
    private static void AppendReleaseBuild(StringBuilder builder, ref bool done)
    {
        done = true;

        builder.Append(" Release Build");
    }

    [Conditional("MSIX_DEBUG")]
    private static void AppendMsixDebugBuild(StringBuilder builder, ref bool done)
    {
        done = true;

        builder.Append(" MSIX Debug Build");
    }

    [Conditional("MSIX")]
    private static void AppendMsixBuild(StringBuilder builder, ref bool done)
    {
        done = true;

        builder.Append(" MSIX Build");
    }

    [Conditional("STEAM")]
    private static void AppendSteamBuild(StringBuilder builder, ref bool done)
    {
        done = true;

        builder.Append(" Steam Build");
    }
}
