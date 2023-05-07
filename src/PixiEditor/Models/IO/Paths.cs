using System.IO;
using System.Reflection;

namespace PixiEditor.Models.IO;
public static class Paths
{
    public static string DataFullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Data");
}
