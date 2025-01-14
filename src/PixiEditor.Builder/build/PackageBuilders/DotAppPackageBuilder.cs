using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Cake.Common;
using Cake.Common.IO;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Path = System.IO.Path;

namespace PixiEditor.Cake.Builder.PackageBuilders;

[PackageInfo("DotApp")]
public sealed class DotAppPackageBuilder : PackageBuilder
{
    public override PackageBuildInfo BuildPackage(BuildContext context)
    {
        string universalBinPath = CombineUniversalBinary(context);
        string plistPath = Path.Combine(context.PackageProjectPath, "Info.plist");

        if (!File.Exists(plistPath))
        {
            return PackageBuildInfo.Fail($"Info.plist not found at {plistPath}");
        }

        string packagePath = Path.Combine(context.OutputDirectory, "package");
        CreateDirectoryIfMissing(packagePath);

        string path = Path.Combine(packagePath, "PixiEditor.app");
        CreateDirectoryIfMissing(path);

        string contentsPath = Path.Combine(path, "Contents");
        CreateDirectoryIfMissing(contentsPath);

        string resourcesPath = Path.Combine(contentsPath, "Resources");
        CreateDirectoryIfMissing(resourcesPath);

        string macOsPath = Path.Combine(contentsPath, "MacOS");
        CreateDirectoryIfMissing(macOsPath);

        string codeSignaturesPath = Path.Combine(contentsPath, "_CodeSignature");
        CreateDirectoryIfMissing(codeSignaturesPath);

        string dllContentsPath = universalBinPath;
        CopyFilesOverwrite(dllContentsPath, macOsPath, true, packagePath);

        string pilst = File.ReadAllText(plistPath);

        string version = ReadVersionFile(context.VersionFile);

        pilst = pilst.Replace("{version-string}", version);

        string targetPilstPath = Path.Combine(contentsPath, "Info.plist");

        File.WriteAllText(targetPilstPath, pilst);

        File.Copy(Path.Combine(context.PackageProjectPath, "PixiEditor.icns"),
            Path.Combine(resourcesPath, "PixiEditor.icns"), true);

        return PackageBuildInfo.Succeed(path);
    }

    private string ReadVersionFile(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Version file not found", path);
        }

        string[] assemblyInfo = File.ReadAllLines(path);

        for (int i = assemblyInfo.Length - 1; i >= 0; i--)
        {
            string line = assemblyInfo[i];
            if (line.Contains("AssemblyVersion"))
            {
                return line.Split('"')[1];
            }
        }

        throw new InvalidOperationException("Version not found in version file");
    }

    private string CombineUniversalBinary(BuildContext context)
    {
        string outputPath = Path.Combine(context.OutputDirectory, "universal");
        CreateDirectoryIfMissing(outputPath);

        StringBuilder args = new StringBuilder();
        args.Append($"-output {Path.Combine(outputPath, "PixiEditor")} ");
        args.Append("-create ");
        foreach (var dir in context.FinalOutputDirectories)
        {
            args.Append($"{Path.Combine(dir, "PixiEditor")} ");
        }

        context.StartProcess("lipo", $"-create {args}");

        return outputPath;
    }
}
