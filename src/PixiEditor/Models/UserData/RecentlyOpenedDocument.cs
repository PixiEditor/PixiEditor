using System.Diagnostics;
using System.IO;
using System.Linq;
using ChunkyImageLib;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.Exceptions;
using PixiEditor.Helpers;
using PixiEditor.Models.IO;
using PixiEditor.Numerics;
using PixiEditor.Parser;

namespace PixiEditor.Models.UserData;

[DebuggerDisplay("{FilePath}")]
internal class RecentlyOpenedDocument : ObservableObject
{
    private bool corrupt;

    private string filePath;

    private Texture previewBitmap;

    public string FilePath
    {
        get => filePath;
        set
        {
            SetProperty(ref filePath, value);
            this.OnPropertyChanged(nameof(FileName));
            this.OnPropertyChanged(nameof(FileExtension));
        }
    }

    public bool Corrupt
    {
        get => corrupt;
        set => SetProperty(ref corrupt, value);
    }

    public string FileName => Path.GetFileNameWithoutExtension(filePath);

    public string FileExtension
    {
        get
        {
            if (!File.Exists(FilePath))
            {
                return "? (Not found)";
            }

            if (Corrupt)
            {
                return "? (Corrupt)";
            }

            string extension = Path.GetExtension(filePath).ToLower();
            return SupportedFilesHelper.IsExtensionSupported(extension) ? extension : $"? ({extension})";
        }
    }

    public RecentlyOpenedDocument(string path)
    {
        FilePath = path;
    }
}
