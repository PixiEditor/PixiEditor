using System.IO;
using Cake.Common.Build;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Publish;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Frosting;
using Path = System.IO.Path;

namespace PixiEditor.Cake.Builder;

public static class Program
{
    public static int Main(string[] args)
    {
        return new CakeHost()
            .UseContext<BuildContext>()
            .Run(args);
    }
}

public class BuildContext : FrostingContext
{
    public string PathToProject { get; set; } = "../PixiEditor/PixiEditor.csproj";

    public string[] ExtensionProjectsToInclude { get; set; } = [];

    public string CrashReportWebhookUrl { get; set; }

    public string AnalyticsUrl { get; set; }

    public string BackedUpConstants { get; set; }

    public string BuildConfiguration { get; set; } = "Release";

    public string OutputDirectory { get; set; } = "Builds";

    public bool SelfContained { get; set; } = false;

    public string Runtime { get; set; }

    public BuildContext(ICakeContext context)
        : base(context)
    {
        CrashReportWebhookUrl = GetArgumentOrDefault(context, "crash-report-webhook-url", string.Empty);
        AnalyticsUrl = GetArgumentOrDefault(context, "analytics-url", string.Empty);

        bool hasCustomProjectPath = context.Arguments.HasArgument("project-path");
        if (hasCustomProjectPath)
        {
            PathToProject = context.Arguments.GetArgument("project-path");
        }

        bool hasCustomExtensionProjects = context.Arguments.HasArgument("extension-projects");
        if (hasCustomExtensionProjects)
        {
            ExtensionProjectsToInclude = context.Arguments.GetArgument("extension-projects").Split(';');
        }

        bool hasCustomConfiguration = context.Arguments.HasArgument("build-configuration");
        if (hasCustomConfiguration)
        {
            BuildConfiguration = context.Arguments.GetArgument("build-configuration");
        }

        bool hasCustomOutputDirectory = context.Arguments.HasArgument("o");
        if (hasCustomOutputDirectory)
        {
            OutputDirectory = context.Arguments.GetArgument("o");
        }

        bool hasSelfContained = context.Arguments.HasArgument("self-contained");
        if (hasSelfContained)
        {
            SelfContained = true;
        }

        Runtime = context.Arguments.GetArgument("runtime");
    }

    private static string GetArgumentOrDefault(ICakeContext context, string argumentName, string defaultValue)
    {
        var arguments = context.Arguments;

        var hasArgument = arguments.HasArgument(argumentName);
        return hasArgument ? arguments.GetArgument(argumentName) : defaultValue;
    }
}

[TaskName("Default")]
[IsDependentOn(typeof(CopyExtensionsTask))]
public sealed class DefaultTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.Log.Information("Built project successfully!");
    }
}

[TaskName("ReplaceSpecialStrings")]
public sealed class ReplaceSpecialStringsTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.Log.Information("Replacing special strings...");
        string projectPath = context.PathToProject;
        string filePath = Path.Combine(projectPath, "..", "PixiEditor", "BuildConstants.cs");

        string result;
        var fileContent = File.ReadAllText(filePath);
        context.BackedUpConstants = fileContent;
        result = ReplaceSpecialStrings(context, fileContent);

        File.WriteAllText(filePath, result);
    }

    private string ReplaceSpecialStrings(BuildContext context, string fileContent)
    {
        string result = fileContent
            .Replace("${crash-report-webhook-url}", context.CrashReportWebhookUrl)
            .Replace("${analytics-url}", context.AnalyticsUrl);

        return result;
    }
}

[TaskName("BuildProject")]
[IsDependentOn(typeof(ReplaceSpecialStringsTask))]
public sealed class BuildProjectTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.Log.Information("Building project...");
        string projectPath = context.PathToProject;

        var settings = new DotNetPublishSettings()
        {
            Configuration = context.BuildConfiguration,
            SelfContained = context.SelfContained,
            Runtime = context.Runtime,
            OutputDirectory = context.OutputDirectory,
        };

        context.DotNetPublish(projectPath, settings);
    }

    public override void Finally(BuildContext context)
    {
        context.Log.Information("Cleaning up...");
        string constantsPath = Path.Combine(context.PathToProject, "..", "PixiEditor", "BuildConstants.cs");

        File.WriteAllText(constantsPath, context.BackedUpConstants);
    }
}

[TaskName("BuildExtensions")]
[IsDependentOn(typeof(BuildProjectTask))]
public sealed class BuildExtensionsTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.Log.Information("Building extensions...");
        foreach (var project in context.ExtensionProjectsToInclude)
        {
            var settings = new DotNetPublishSettings() { Configuration = context.BuildConfiguration, };

            context.DotNetPublish(project, settings);
        }
    }
}

[TaskName("CopyExtensions")]
[IsDependentOn(typeof(BuildExtensionsTask))]
public sealed class CopyExtensionsTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.Log.Information("Copying extensions...");
        foreach (var project in context.ExtensionProjectsToInclude)
        {
            string outputDir = Path.Combine(context.OutputDirectory, "Extensions");
            string sourceDir = Path.Combine(project, "bin",
                context.BuildConfiguration, "wasi-wasm", "Extensions");

            CopyDirectoryContents(sourceDir, outputDir, context);
        }
    }

    private void CopyDirectoryContents(string sourceDir, string targetDir, BuildContext context)
    {
        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        context.Log.Information($"Copying contents of {sourceDir} to {targetDir}");

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            string targetFile = Path.Combine(targetDir, Path.GetFileName(file));
            context.Log.Information($"Copying {file} to {targetFile}");
            File.Copy(file, targetFile, true);
        }
    }
}
