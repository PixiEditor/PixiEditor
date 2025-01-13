using System;

namespace PixiEditor.Cake.Builder.PackageBuilders;

[AttributeUsage(AttributeTargets.Class)]
public class PackageInfoAttribute : Attribute
{
    public string PackageName { get; }

    public PackageInfoAttribute(string packageName)
    {
        PackageName = packageName;
    }
}
