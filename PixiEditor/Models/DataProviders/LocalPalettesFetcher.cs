using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.DataHolders.Palettes;
using PixiEditor.Models.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PixiEditor.Models.DataProviders
{
    public class LocalPalettesFetcher : PaletteListDataSource
    {
        public string PathToPalettesFolder { get; private set; } = Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PixiEditor", "Palettes");

        public override async Task<PaletteList> FetchPaletteList(int startIndex, int count)
        {
            string[] files = DirectoryExtensions.GetFiles(PathToPalettesFolder, string.Join("|", AvailableParsers.SelectMany(x => x.SupportedFileExtensions)), SearchOption.TopDirectoryOnly);

            PaletteList result = new PaletteList
            {
                Palettes = new WpfObservableRangeCollection<Palette>()
            };

            if (startIndex >= files.Length) return result;

            for (int i = 0; i < count; i++)
            {
                if (startIndex + i >= files.Length) break;
                string filePath = files[i + startIndex];
                string extension = Path.GetExtension(filePath);
                var foundParser = AvailableParsers.First(x => x.SupportedFileExtensions.Contains(extension));
                if (foundParser != null)
                {
                    PaletteFileData fileData = await foundParser.Parse(filePath);
                    result.Palettes.Add(new Palette(fileData.Title, new List<string>(fileData.GetHexColors()), fileData.Tags));
                }
            }

            result.FetchedCorrectly = true;
            return result;
        }

        public override void Initialize()
        {
            if(!Directory.Exists(PathToPalettesFolder))
            {
                Directory.CreateDirectory(PathToPalettesFolder);
            }
        }
    }
}
