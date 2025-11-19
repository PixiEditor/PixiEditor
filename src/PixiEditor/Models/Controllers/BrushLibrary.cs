using Avalonia.Platform;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;
using PixiEditor.Helpers;
using PixiEditor.Models.BrushEngine;
using PixiEditor.Models.IO;
using PixiEditor.ViewModels.Document.Nodes.Brushes;

namespace PixiEditor.Models.Controllers;

internal class BrushLibrary
{
    private Dictionary<Guid, Brush> brushes = new();
    public IReadOnlyDictionary<Guid, Brush> Brushes => brushes;

    private string pathToBrushes;
    public string PathToBrushes => pathToBrushes;

    public event Action BrushesChanged;

    public BrushLibrary(string pathToBrushes)
    {
        this.pathToBrushes = pathToBrushes;
    }

    private void LoadBuiltIn()
    {
        Uri brushesUri = new Uri("avares://PixiEditor/Data/Brushes/");
        var assets = AssetLoader.GetAssets(brushesUri, null);

        foreach (var asset in assets)
        {
            string localPath = asset.LocalPath;
            if (localPath.EndsWith(".pixi", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var fullUri = new Uri(brushesUri, asset);
                    using var stream = AssetLoader.Open(fullUri);
                    byte[] buffer = new byte[stream.Length];
                    stream.ReadExactly(buffer, 0, buffer.Length);
                    var doc = Importer.ImportDocument(buffer, null);

                    using var graph = doc.ShareGraph();
                    BrushOutputNode outputNode =
                        graph.TryAccessData().AllNodes.OfType<BrushOutputNode>().FirstOrDefault();
                    string name = Path.GetFileNameWithoutExtension(localPath);
                    if (outputNode != null)
                    {
                        name = outputNode.BrushName.Value;
                    }

                    var brush = new Brush(name, doc);
                    brushes.Add(brush.Id, brush);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load built-in brush from {asset}: {ex.Message}");
                }
            }
        }
    }

    private void LoadBrushesFromPath(string path)
    {
        if (!Directory.Exists(path))
            return;

        var brushFiles = Directory.GetFiles(path, "*.pixi", SearchOption.AllDirectories);
        foreach (var file in brushFiles)
        {
            try
            {
                var doc = Importer.ImportDocument(file, false);
                var brush = new Brush(Path.GetFileNameWithoutExtension(file), doc);
                brushes.Add(brush.Id, brush);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load brush from {file}: {ex.Message}");
            }
        }
    }

    public void LoadBrushes()
    {
        LoadBuiltIn();
        LoadBrushesFromPath(pathToBrushes);

        BrushesChanged?.Invoke();
    }

    public void Add(Brush brush)
    {
        var oldBrushes = Brushes.Values.ToList();
        if (brushes.TryAdd(brush.Id, brush))
        {
            BrushesChanged?.Invoke();
        }
    }

    public void RemoveById(Guid infoId)
    {
        brushes.Remove(infoId);
        BrushesChanged?.Invoke();
    }
}
