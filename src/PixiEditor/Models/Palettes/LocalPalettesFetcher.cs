using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PixiEditor.Extensions.CommonApi.Async;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Extensions.CommonApi.Palettes.Parsers;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.IO;
using PixiEditor.Models.IO.PaletteParsers.JascPalFile;

namespace PixiEditor.Models.Palettes;

internal delegate void CacheUpdate(RefreshType refreshType, Palette itemAffected, string oldName);

internal class LocalPalettesFetcher : PaletteListDataSource
{
    private List<Palette> cachedPalettes;

    public event CacheUpdate CacheUpdated;

    private List<string> cachedFavoritePalettes;

    private FileSystemWatcher watcher;

    public LocalPalettesFetcher() : base("LOCAL_PALETTE_SOURCE_NAME")
    {
    }

    public override void Initialize()
    {
        InitDir();
        watcher = new FileSystemWatcher(Paths.PathToPalettesFolder);
        watcher.Filter = "*.pal";
        watcher.Changed += FileSystemChanged;
        watcher.Deleted += FileSystemChanged;
        watcher.Renamed += RenamedFile;
        watcher.Created += FileSystemChanged;

        watcher.EnableRaisingEvents = true;
        cachedFavoritePalettes = PixiEditorSettings.Palettes.FavouritePalettes.AsList();

        PixiEditorSettings.Palettes.FavouritePalettes.AddListCallback(updated =>
        {
            cachedFavoritePalettes = updated;
            cachedPalettes.ForEach(x => x.IsFavourite = cachedFavoritePalettes.Contains(x.Name));
        });
    }

    public override async AsyncCall<List<IPalette>> FetchPaletteList(int startIndex, int count, FilteringSettings filtering)
    {
        if (cachedPalettes == null)
        {
            await RefreshCacheAll();
        }

        var filteredPalettes = cachedPalettes.Where(filtering.Filter).OrderByDescending(x => x.IsFavourite).ToArray();

        List<IPalette> result = new List<IPalette>();

        if (startIndex >= filteredPalettes.Length) return result;

        for (int i = 0; i < count; i++)
        {
            if (startIndex + i >= filteredPalettes.Length) break;
            Palette palette = filteredPalettes[startIndex + i];
            result.Add(palette);
        }

        return result;
    }

    public static bool PaletteExists(string paletteName)
    {
        string finalFileName = paletteName;
        if (!paletteName.EndsWith(".pal"))
        {
            finalFileName += ".pal";
        }

        return File.Exists(Path.Join(Paths.PathToPalettesFolder, finalFileName));
    }

    public static string GetNonExistingName(string currentName, bool appendExtension = false)
    {
        string newName = Path.GetFileNameWithoutExtension(currentName);

        if (File.Exists(Path.Join(Paths.PathToPalettesFolder, newName + ".pal")))
        {
            int number = 1;
            while (true)
            {
                string potentialName = $"{newName} ({number})";
                number++;
                if (File.Exists(Path.Join(Paths.PathToPalettesFolder, potentialName + ".pal")))
                    continue;
                newName = potentialName;
                break;
            }
        }

        if (appendExtension)
            newName += ".pal";

        return newName;
    }

    public async Task SavePalette(string fileName, PaletteColor[] colors)
    {
        watcher.EnableRaisingEvents = false;
        string path = Path.Join(Paths.PathToPalettesFolder, fileName);
        InitDir();
        await JascFileParser.SaveFile(path, new PaletteFileData(colors));
        watcher.EnableRaisingEvents = true;

        await RefreshCache(RefreshType.Created, path);
    }

    public async Task DeletePalette(string name)
    {
        if (!Directory.Exists(Paths.PathToPalettesFolder)) return;
        string path = Path.Join(Paths.PathToPalettesFolder, name);
        if (!File.Exists(path)) return;

        watcher.EnableRaisingEvents = false;
        File.Delete(path);
        watcher.EnableRaisingEvents = true;

        await RefreshCache(RefreshType.Deleted, path);
    }

    public void RenamePalette(string oldFileName, string newFileName)
    {
        if (!Directory.Exists(Paths.PathToPalettesFolder))
            return;

        string oldPath = Path.Join(Paths.PathToPalettesFolder, oldFileName);
        string newPath = Path.Join(Paths.PathToPalettesFolder, newFileName);
        if (!File.Exists(oldPath) || File.Exists(newPath))
            return;

        watcher.EnableRaisingEvents = false;
        File.Move(oldPath, newPath);
        watcher.EnableRaisingEvents = true;

        RefreshCacheRenamed(newPath, oldPath);
    }

    public async Task RefreshCacheAll()
    {
        string[] files = DirectoryExtensions.GetFiles(
            Paths.PathToPalettesFolder,
            string.Join("|", AvailableParsers.SelectMany(x => x.SupportedFileExtensions).Distinct()),
            SearchOption.TopDirectoryOnly);
        cachedPalettes = await ParseAll(files);
        CacheUpdated?.Invoke(RefreshType.All, null, null);
    }

    private async void FileSystemChanged(object sender, FileSystemEventArgs e)
    {
        bool waitableExceptionOccured = false;
        do
        {
            try
            {
                switch (e.ChangeType)
                {
                    case WatcherChangeTypes.Created:
                        await RefreshCache(RefreshType.Created, e.FullPath);
                        break;
                    case WatcherChangeTypes.Deleted:
                        await RefreshCache(RefreshType.Deleted, e.FullPath);
                        break;
                    case WatcherChangeTypes.Changed:
                        await RefreshCache(RefreshType.Updated, e.FullPath);
                        break;
                    case WatcherChangeTypes.Renamed:
                        // Handled by method below
                        break;
                    case WatcherChangeTypes.All:
                        await RefreshCache(RefreshType.Created, e.FullPath);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                waitableExceptionOccured = false;
            }
            catch (IOException)
            {
                waitableExceptionOccured = true;
                await Task.Delay(100);
            }

        }
        while (waitableExceptionOccured);
    }

    private async Task RefreshCache(RefreshType refreshType, string file)
    {
        Palette updated = null;
        string affectedFileName = null;

        if (cachedPalettes == null)
        {
            await RefreshCacheAll();
            return;
        }
        
        

        switch (refreshType)
        {
            case RefreshType.All:
                throw new ArgumentException("To handle refreshing all items, use RefreshCacheAll");
            case RefreshType.Created:
                updated = await RefreshCacheAdded(file);
                break;
            case RefreshType.Updated:
                updated = await RefreshCacheUpdated(file);
                break;
            case RefreshType.Deleted:
                affectedFileName = RefreshCacheDeleted(file);
                break;
            case RefreshType.Renamed:
                throw new ArgumentException("To handle renaming, use RefreshCacheRenamed");
            default:
                throw new ArgumentOutOfRangeException(nameof(refreshType), refreshType, null);
        }

        if (refreshType is RefreshType.Created or RefreshType.Updated && updated == null)
        {
            await RefreshCacheAll();
            
            // Using try-catch to generate stack trace
            try
            {
                throw new NullReferenceException($"The '{nameof(updated)}' was null even though the refresh type was '{refreshType}'.");
            }
            catch (Exception e)
            {
                await CrashHelper.SendExceptionInfoAsync(e);
            }

            return;
        }
        
        CacheUpdated?.Invoke(refreshType, updated, affectedFileName);
    }

    private void RefreshCacheRenamed(string newFilePath, string oldFilePath)
    {
        string oldFileName = Path.GetFileName(oldFilePath);
        int index = cachedPalettes.FindIndex(p => p.FileName == oldFileName);
        if (index == -1) return;

        Palette palette = cachedPalettes[index];
        palette.FileName = Path.GetFileName(newFilePath);
        palette.Name = Path.GetFileNameWithoutExtension(newFilePath);

        CacheUpdated?.Invoke(RefreshType.Renamed, palette, oldFileName);
    }

    private string RefreshCacheDeleted(string filePath)
    {
        string fileName = Path.GetFileName(filePath);
        int index = cachedPalettes.FindIndex(p => p.FileName == fileName);
        if (index == -1) return null;

        cachedPalettes.RemoveAt(index);
        return fileName;
    }

    private async Task<Palette> RefreshCacheItem(string file, Action<Palette> action)
    {
        if (File.Exists(file))
        {
            string extension = Path.GetExtension(file);
            var foundParser = AvailableParsers.FirstOrDefault(x => x.SupportedFileExtensions.Contains(extension));
            if (foundParser != null)
            {
                var newPalette = await foundParser.Parse(file);
                if (newPalette is { IsCorrupted: false })
                {
                    Palette pal = CreatePalette(newPalette, file,
                        cachedFavoritePalettes?.Contains(newPalette.Title) ?? false);
                    action(pal);

                    return pal;
                }
            }
        }

        return null;
    }

    private async Task<Palette> RefreshCacheUpdated(string file)
    {
        return await RefreshCacheItem(file, palette =>
        {
            Palette existingPalette = cachedPalettes.FirstOrDefault(x => x.FileName == palette.FileName);
            if (existingPalette != null)
            {
                existingPalette.Colors = palette.Colors.ToList();
                existingPalette.Name = palette.Name;
                existingPalette.FileName = palette.FileName;
            }
        });
    }

    private async Task<Palette> RefreshCacheAdded(string file)
    {
        return await RefreshCacheItem(file, palette =>
        {
            string fileName = Path.GetFileName(file);
            int index = cachedPalettes.FindIndex(p => p.FileName == fileName);
            if (index != -1)
            {
                cachedPalettes.RemoveAt(index);
            }

            cachedPalettes.Add(palette);
        });
    }

    private async Task<List<Palette>> ParseAll(string[] files)
    {
        List<Palette> result = new List<Palette>();

        foreach (var file in files)
        {
            string extension = Path.GetExtension(file);
            if (!File.Exists(file)) continue;
            var foundParser = AvailableParsers.First(x => x.SupportedFileExtensions.Contains(extension));
            {
                PaletteFileData fileData = await foundParser.Parse(file);
                if (fileData.IsCorrupted) continue;
                var palette = CreatePalette(fileData, file, cachedFavoritePalettes?.Contains(fileData.Title) ?? false);

                result.Add(palette);
            }
        }

        return result;
    }

    private Palette CreatePalette(PaletteFileData fileData, string file, bool isFavourite)
    {
        var palette = new Palette(
            fileData.Title,
            new List<PaletteColor>(fileData.GetPaletteColors()),
            Path.GetFileName(file), this)
        {
            IsFavourite = isFavourite
        };

        return palette;
    }

    private void RenamedFile(object sender, RenamedEventArgs e)
    {
        RefreshCacheRenamed(e.FullPath, e.OldFullPath);
    }

    private static void InitDir()
    {
        if (!Directory.Exists(Paths.PathToPalettesFolder))
        {
            Directory.CreateDirectory(Paths.PathToPalettesFolder);
        }
    }
}
