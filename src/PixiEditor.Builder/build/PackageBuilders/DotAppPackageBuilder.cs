using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using Cake.Core.Diagnostics;

namespace PixiEditor.Cake.Builder.PackageBuilders;

[PackageInfo("DotApp")]
public sealed class DotAppPackageBuilder : PackageBuilder
{
    public override PackageBuildInfo BuildPackage(BuildContext context)
    {
        string plistPath = Path.Combine(context.PackageProjectPath, "Info.plist");

        if (!File.Exists(plistPath))
        {
            return PackageBuildInfo.Fail($"Info.plist not found at {plistPath}");
        }

        string packagePath = Path.Combine(context.OutputDirectory, "package");
        
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

        string dllContentsPath = context.OutputDirectory;
        CopyFilesOverwrite(dllContentsPath, macOsPath, true, packagePath);
        
        string pilst = File.ReadAllText(plistPath);
        
        var assembly = Assembly.LoadFile(Path.Combine(dllContentsPath, "PixiEditor.dll"));
        
        pilst = pilst.Replace("{version-string}", assembly.GetName().Version.ToString());

        string targetPilstPath = Path.Combine(contentsPath, "Info.plist");
        
        File.WriteAllText(targetPilstPath, pilst);

        File.Copy(Path.Combine(context.PackageProjectPath, "PixiEditor.icns"),
            Path.Combine(resourcesPath, "PixiEditor.icns"), true);

        return PackageBuildInfo.Succeed(path);
    }
}
