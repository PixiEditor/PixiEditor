using System;
using System.Linq;
using System.Reflection;
using Cake.Common;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Frosting;
using PixiEditor.Cake.Builder.PackageBuilders;

namespace PixiEditor.Cake.Builder;

[IsDependentOn(typeof(CopyExtensionsTask))]
public sealed class CreateAppPackageTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context)
    {
        return context.BuildPackage;
    }

    public override void Run(BuildContext context)
    {
        var builders = typeof(CreateAppPackageTask).Assembly.GetTypes()
            .Select(x => (x, x.GetCustomAttribute<PackageInfoAttribute>())).Where(x => x.Item2 != null);

        foreach (var builder in builders)
        {
            if (string.Equals(builder.Item2.PackageName, context.PackageType, StringComparison.InvariantCultureIgnoreCase))
            {
                var packageBuilder = (PackageBuilder)Activator.CreateInstance(builder.x);
                if (packageBuilder == null)
                {
                    throw new InvalidOperationException($"Could not create instance of {builder.x.Name}");
                }

                context.Log.Information($"Building package {builder.Item2.PackageName}");
                var info = packageBuilder.BuildPackage(context);
                if (info.Success)
                {
                    context.Log.Information($"Package built successfully to {info.PathToPackage}");
                }
                else
                {
                    context.Log.Error($"Failed to build package {builder.Item2.PackageName} with error: {info.Error}");
                }
            }
        }
    }
}
