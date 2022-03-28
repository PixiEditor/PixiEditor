using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.DataHolders.Palettes;
using PixiEditor.Models.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PixiEditor.Models.IO.JascPalFile;
using PixiEditor.Models.UserPreferences;
using SkiaSharp;

namespace PixiEditor.Models.DataProviders
{
    public class LocalPalettesFetcher : PaletteListDataSource
    {
        public static string PathToPalettesFolder { get; } = Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PixiEditor", "Palettes");

        public List<Palette> CachedPalettes { get; private set; }

        public override async Task<PaletteList> FetchPaletteList(int startIndex, int count, FilteringSettings filtering)
        {
            if(CachedPalettes == null)
            {
                await RefreshCache();
            }

            PaletteList result = new PaletteList
            {
                Palettes = new WpfObservableRangeCollection<Palette>()
            };

            var filteredPalettes = CachedPalettes.Where(filtering.Filter).ToArray();

            if (startIndex >= filteredPalettes.Length) return result;

            for (int i = 0; i < count; i++)
            {
                if (startIndex + i >= filteredPalettes.Length) break;
                Palette palette = filteredPalettes[startIndex + i];
                result.Palettes.Add(palette);
            }

            result.FetchedCorrectly = true;
            return result;
        }

        public async Task RefreshCache()
        {
            string[] files = DirectoryExtensions.GetFiles(PathToPalettesFolder, string.Join("|", AvailableParsers.SelectMany(x => x.SupportedFileExtensions)), SearchOption.TopDirectoryOnly);
            CachedPalettes = await ParseAll(files);
        }

        private async Task<List<Palette>> ParseAll(string[] files)
        {
            List<Palette> result = new List<Palette>();
            foreach (var file in files)
            {
                string extension = Path.GetExtension(file);
                var foundParser = AvailableParsers.First(x => x.SupportedFileExtensions.Contains(extension));
                {
                    PaletteFileData fileData = await foundParser.Parse(file);
                    var palette = new Palette(
                        fileData.Title,
                        new List<string>(fileData.GetHexColors()),
                        Path.GetFileName(file));
                    List<string> favouritePalettes = IPreferences.Current.GetLocalPreference<List<string>>(PreferencesConstants.FavouritePalettes);
                    if (favouritePalettes != null)
                    {
                        palette.IsFavourite = favouritePalettes.Contains(palette.Name);
                    }

                    result.Add(palette);
                }
            }

            return result;
        }

        public static async Task SavePalette(string fileName, SKColor[] colors)
        {
            string path = Path.Join(PathToPalettesFolder, fileName);
            InitDir();
            await JascFileParser.SaveFile(path, new PaletteFileData(colors));
        }

        public static void DeletePalette(string name)
        {
            if (!Directory.Exists(PathToPalettesFolder)) return;
            string path = Path.Join(PathToPalettesFolder, name);
            if (!File.Exists(path)) return;

            File.Delete(path);
        }

        public override void Initialize()
        {
            InitDir();
        }

        private static void InitDir()
        {
            if (!Directory.Exists(PathToPalettesFolder))
            {
                Directory.CreateDirectory(PathToPalettesFolder);
            }
        }
    }
}
