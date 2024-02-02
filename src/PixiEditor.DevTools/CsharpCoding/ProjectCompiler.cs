using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Build.Framework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;

namespace PixiEditor.DevTools.CsharpCoding;

public class ProjectCompiler
{
    public MSBuildWorkspace Workspace { get; }
    public List<Project> CsProjects { get; private set; }

    public List<Type> AnimacoProjectsTypes = new List<Type>();
    public Assembly? CompiledAssembly { get; private set; }

    private Dictionary<Document, SyntaxTree> _cachedSyntaxTrees = new Dictionary<Document, SyntaxTree>();
    private Compilation? _cachedCompilation;
    private WeakReference _weakRef;

    public ProjectCompiler(MSBuildWorkspace workspace, List<Project> projects)
    {
        Workspace = workspace;
        CsProjects = projects;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public async Task<Assembly?> Compile(bool restore = false)
    {
        return await Compile(await GetDocuments());
        CompiledAssembly = await CliCompile(restore);
        if (CompiledAssembly != null)
        {
            AnimacoProjectsTypes = CompiledAssembly.GetTypes().Where(x => typeof(AnimacoProject).IsAssignableFrom(x))
                .ToList();
        }

        return CompiledAssembly;
    }

    private async Task<Assembly?> CliCompile(bool restore)
    {
        var project = CsProjects[^1];
        if (restore)
        {
            RestorePackages(project);
        }

        // Run dotnet msbuild
        ProcessStartInfo startInfo = new ProcessStartInfo("dotnet",
            $"msbuild {project.FilePath} -p:Configuration=Release -p:Platform=x64 -p:OutputPath={Path.GetDirectoryName(project.OutputRefFilePath)}");
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.UseShellExecute = false;
        Process process = new Process();
        process.StartInfo = startInfo;
        process.OutputDataReceived += (sender, args) => ILogger.Current.Log(args.Data);
        process.ErrorDataReceived += (sender, args) => ILogger.Current.LogError(args.Data);
        process.Start();

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            return null;
        }

        return Assembly.LoadFrom(project.OutputRefFilePath);
    }

    private void RestorePackages(Project project)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo("dotnet", $"restore {project.FilePath}")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        Process process = new Process();
        process.StartInfo = startInfo;
        process.OutputDataReceived += (sender, args) => ILogger.Current.Log(args.Data);
        process.ErrorDataReceived += (sender, args) => ILogger.Current.LogError(args.Data);
        process.Start();

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
    }

    private async Task<HashSet<Document>> GetDocuments()
    {
        HashSet<Document> documents = new HashSet<Document>();
        for (var i = 0; i < CsProjects.Count; i++)
        {
            var project = CsProjects[i];
            var generated = await project.GetSourceGeneratedDocumentsAsync();
            List<Document> allDocs = new List<Document>(project.Documents.Concat(generated));

            foreach (var document in allDocs)
            {
                if (documents.Contains(document) || (i < CsProjects.Count - 1 && IsAssemblyInfo(document.FilePath)))
                    continue;

                documents.Add(document);
            }
        }

        return documents;
    }

    private bool IsAssemblyInfo(string documentFilePath)
    {
        return documentFilePath.EndsWith("AssemblyInfo.cs") || documentFilePath.EndsWith("AssemblyAttributes.cs");
    }

    public async Task<Assembly?> Compile(HashSet<Document> documents)
    {
        await ParseSyntaxTrees(documents);

        var references = CreateReferences();

        await CreateCompilation(documents, references);
        List<ResourceDescription> manifestResources = GetManifestResources();

        using var ms = new MemoryStream();
        EmitResult result = _cachedCompilation.Emit(ms, manifestResources: manifestResources);
        LogDiagnostics(result);
        if (!result.Success)
        {
            IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                diagnostic.IsWarningAsError ||
                diagnostic.Severity == DiagnosticSeverity.Error);

            foreach (Diagnostic diagnostic in failures)
            {
                ILogger.Current.LogError($"{diagnostic.Id}: {diagnostic.GetMessage()}");
            }
        }
        else
        {
            LoadCompiledAssembly(ms);
        }

        return CompiledAssembly;
    }

    private void LogDiagnostics(EmitResult result)
    {
        foreach (var diagnostic in result.Diagnostics)
        {
            if (diagnostic.Severity == DiagnosticSeverity.Error)
                ILogger.Current.LogError(diagnostic.GetMessage());
            else
                ILogger.Current.Log(diagnostic.GetMessage());
        }
    }

    private List<ResourceDescription> GetManifestResources()
    {
        /*TODO: Doesn't work for precompiled XAML sadly*/
        List<ResourceDescription> manifestResources = new List<ResourceDescription>();
        foreach (var project in CsProjects)
        {
            string dllDir = Path.GetDirectoryName(project.OutputRefFilePath);
            string objDir = Path.Combine(dllDir, "..");
            string avaloniaResources = Path.Combine(objDir, "Avalonia");
            if (Directory.Exists(avaloniaResources))
            {
                foreach (var file in Directory.GetFiles(avaloniaResources))
                {
                    string fileName = Path.GetFileName(file);
                    if (fileName == "resources")
                    {
                        manifestResources.Add(new ResourceDescription(
                            "!AvaloniaResources", () => File.OpenRead(file), true));
                    }
                }
            }
        }

        return manifestResources;
    }

    private async Task ParseSyntaxTrees(HashSet<Document> documents)
    {
        foreach (var document in documents)
        {
            SourceText source = await document.GetTextAsync();
            _cachedSyntaxTrees[document] = CSharpSyntaxTree.ParseText(source);
        }
    }

    private List<MetadataReference> CreateReferences()
    {
        List<MetadataReference> references = new List<MetadataReference>();
        references.AddRange(CsProjects.SelectMany(x => x.MetadataReferences));
        references.Add(MetadataReference.CreateFromFile(Assembly.GetExecutingAssembly().Location));

        return references.Distinct().ToList();
    }

    private void LoadCompiledAssembly(MemoryStream ms)
    {
        ms.Seek(0, SeekOrigin.Begin);
        CompiledAssembly = Assembly.Load(ms.ToArray());
        SaveAssembly(ms);
        AnimacoProjectsTypes = CompiledAssembly.GetTypes().Where(x => typeof(AnimacoProject).IsAssignableFrom(x))
            .ToList();
    }

    private void SaveAssembly(MemoryStream ms)
    {
        string path = Path.Combine(Path.GetDirectoryName(CsProjects[^1].OutputRefFilePath)!, "T.Animaco.Examples.dll");
        File.WriteAllBytes(path, ms.ToArray());
    }

    private Assembly? CurrentDomainOnAssemblyResolve(object? sender, ResolveEventArgs args)
    {
        string dllDir = Path.GetDirectoryName(CsProjects[^1].OutputRefFilePath)!;
        string assemblyPath = Path.Combine(dllDir, $"{args.Name.Split(',')[0]}.dll");
        if (File.Exists(assemblyPath))
        {
            return Assembly.LoadFrom(assemblyPath);
        }

        return null;
    }

    private async Task CreateCompilation(HashSet<Document> documents, List<MetadataReference> references)
    {
        if (_cachedCompilation != null)
        {
            foreach (var document in documents)
            {
                SyntaxTree? oldSyntaxTree =
                    _cachedCompilation.SyntaxTrees.FirstOrDefault(x => x.FilePath == document.FilePath);
                if (oldSyntaxTree != null)
                {
                    _cachedCompilation =
                        _cachedCompilation.ReplaceSyntaxTree(oldSyntaxTree, _cachedSyntaxTrees[document]);
                }
                else
                {
                    _cachedCompilation = _cachedCompilation.AddSyntaxTrees(_cachedSyntaxTrees[document]);
                }
            }
        }
        else
        {
            _cachedCompilation = await CsProjects[^1].GetCompilationAsync(); /*CSharpCompilation.Create(
                CsProjects[^1].Name,
                _cachedSyntaxTrees.Values,
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));*/
        }
    }

    public AnimacoProject GetProject(string csFileName)
    {
        if (CompiledAssembly == null)
        {
            throw new InvalidOperationException("Project must be compiled first.");
        }

        int projIndex = GetProjectIndex(csFileName);

        if (projIndex == -1)
        {
            throw new ProjectNotFoundException($"No project with name {csFileName} found");
        }

        return (AnimacoProject)Activator.CreateInstance(AnimacoProjectsTypes[projIndex])!;
    }

    public int GetProjectIndex(string csFileName)
    {
        if (CompiledAssembly == null)
        {
            throw new InvalidOperationException("Project must be compiled first.");
        }

        if (string.IsNullOrEmpty(csFileName))
        {
            return 0;
        }

        string fileName = Path.GetFileNameWithoutExtension(csFileName);

        for (int i = 0; i < AnimacoProjectsTypes.Count; i++)
        {
            if (string.Equals(fileName, AnimacoProjectsTypes[i].Name, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }
}
