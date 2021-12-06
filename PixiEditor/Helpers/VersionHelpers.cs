using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace PixiEditor.Helpers
{
    public static class VersionHelpers
    {
        public static Version GetCurrentAssemblyVersion() => Assembly.GetExecutingAssembly().GetName().Version;

        public static string GetCurrentAssemblyVersion(Func<Version, string> toString) => toString(GetCurrentAssemblyVersion());

        public static string GetCurrentAssemblyVersionString()
        {
            StringBuilder builder = new(GetCurrentAssemblyVersion().ToString());

            bool isDone = false;

            AppendDevBuild(builder, ref isDone);

            if (isDone)
            {
                return builder.ToString();
            }

            AppendDebugBuild(builder, ref isDone);

            return builder.ToString();
        }

        [Conditional("DEV_RELEASE")]
        private static void AppendDevBuild(StringBuilder builder, ref bool done)
        {
            done = true;

            builder.Append(" Dev");
        }

        [Conditional("DEBUG")]
        private static void AppendDebugBuild(StringBuilder builder, ref bool done)
        {
            done = true;

            builder.Append(" Debug");
        }
    }
}