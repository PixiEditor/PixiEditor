using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace PixiEditor.Helpers
{
    public static class AssemblyHelper
    {
        public static Version GetCurrentAssemblyVersion() => Assembly.GetExecutingAssembly().GetName().Version;

        public static string GetCurrentAssemblyVersion(Func<Version, string> toString) => toString(GetCurrentAssemblyVersion());
    }
}