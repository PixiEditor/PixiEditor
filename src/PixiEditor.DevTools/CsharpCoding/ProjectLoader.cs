using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace PixiEditor.DevTools.CsharpCoding;

public class ProjectLoader
{
    public MSBuildWorkspace Workspace { get; private set; }
    public string ProjectPath { get; private set; }
    public List<PackageReference> PackageReferences { get; private set; }

    public Project TargetProject { get; private set; }
    public List<Project> AllProjects { get; private set; }

    public List<string> ReferencedProjectPaths { get; private set; }

    private static readonly string[] _coreProjects = new[] { "PixiEditor.AvaloniaUI" };

    public ProjectLoader(string projectPath)
    {
        MSBuildLocator.RegisterDefaults();
        Dictionary<string, string> props = new Dictionary<string, string>();
        Workspace = MSBuildWorkspace.Create(props);
        Workspace.LoadMetadataForReferencedProjects = true;

        PackageReferences = LoadPackageReferences(projectPath, out List<string> projects);
        ReferencedProjectPaths = projects;
        ProjectPath = projectPath;
    }

    private List<PackageReference> LoadPackageReferences(string projectPath, out List<string> projects)
    {
        projects = DigProjectReferences(projectPath, new List<string>());
        List<PackageReference> packageReferences = new List<PackageReference>();
        foreach (var project in projects)
        {
            packageReferences.AddRange(LoadForProject(project, packageReferences));
        }

        return packageReferences;
    }

    private List<string> DigProjectReferences(string projectPath, List<string> existingProjects)
    {
        string xml = File.ReadAllText(projectPath);
        var doc = XDocument.Parse(xml);
        var projectReferences = doc.XPathSelectElements("//ProjectReference")
            .Select(pr => pr.Attribute("Include").Value).Except(existingProjects).ToList();

        foreach (var projectReference in projectReferences)
        {
            string projectFullPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(projectPath)!, projectReference));
            var references = DigProjectReferences(projectFullPath, existingProjects);
            foreach (var reference in references)
            {
                if (!existingProjects.Contains(reference))
                {
                    existingProjects.Add(reference);
                }
            }
        }

        existingProjects.Add(projectPath);
        return existingProjects;
    }

    private static List<PackageReference> LoadForProject(string projectPath, List<PackageReference> existingPackages)
    {
        string xml = File.ReadAllText(projectPath);
        var doc = XDocument.Parse(xml);
        var packageReferences = doc.XPathSelectElements("//PackageReference")
            .Select(pr => new PackageReference
            {
                Include = pr.Attribute("Include").Value,
                Version = new Version(pr.Attribute("Version").Value)
            });

        return packageReferences.Where(pr => existingPackages.All(ep => ep.Include != pr.Include))
            .ToList();
    }

    public async Task LoadProjectsAsync()
    {
        AllProjects = new List<Project>();
        foreach (var projectPath in ReferencedProjectPaths)
        {
            if(IsAnimacoCoreProject(projectPath))
                continue;
            AllProjects.Add(await Workspace.OpenProjectAsync(projectPath));
        }

        TargetProject = AllProjects.First(x => x.FilePath == ProjectPath);
    }

    private bool IsAnimacoCoreProject(string projectPath)
    {
        return _coreProjects.Any(x => projectPath.EndsWith($"{x}.csproj"));
    }
}

public class PackageReference
{
    public string Include { get; set; }
    public Version Version { get; set; }
}
